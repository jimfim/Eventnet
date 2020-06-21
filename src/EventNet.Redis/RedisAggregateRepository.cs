using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventNet.Core;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EventNet.Redis
{
    public class RedisAggregateRepository<TAggregate> : IAggregateRepository<TAggregate> 
        where TAggregate : AggregateRoot
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IAggregateFactory _factory = new AggregateFactory();

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };
        public RedisAggregateRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }
        public async Task SaveAsync(AggregateRoot aggregate)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var events = aggregate.UncommittedEvents;
            if (events.Any() == false)
            {
                return;
            }

            var primaryStream = RedisExtensions.GetPrimaryStreamName();
            var aggregateStreamName = RedisExtensions.GetAggregateStreamName<TAggregate>(aggregate.AggregateId);
            foreach (var @event in events)
            {
                var key = await db.StringIncrementAsync(RedisExtensions.GetStreamIdKey());
                var messageId = $"{key}-0";
                var data = @event.ToJson();
                var primaryStreamTask = db.StreamAddAsync(primaryStream, aggregate.AggregateId.ToString(), data, messageId);
                var aggregateStreamTask = db.StreamAddAsync(aggregateStreamName, aggregate.AggregateId.ToString(), data, messageId);
                await Task.WhenAll(new List<Task>() {primaryStreamTask, aggregateStreamTask});
            }
            aggregate.MarkEventsCompleted();
        }

        public async Task<TAggregate> GetAsync(Guid id)
        {
            var streamName = RedisExtensions.GetAggregateStreamName<TAggregate>(id);
            var db = _connectionMultiplexer.GetDatabase();
            var events = new List<IAggregateEvent>();
            string checkpoint = "0-0";
            int batchSize = 100;
            var info = db.StreamInfo(streamName);
            
            do
            {
                var currentSlice = await db.StreamReadAsync(streamName, checkpoint, batchSize);
                foreach (var streamEntry in currentSlice)
                {
                    foreach (var streamEntryValue in streamEntry.Values)
                    {
                        events.Add(JsonConvert.DeserializeObject<IAggregateEvent>(streamEntryValue.Value.ToString(), _serializerSettings));
                        checkpoint = streamEntry.Id;
                    }
                }
            } while (info.LastEntry.Id !=  checkpoint);
            var aggregate = _factory.Create<TAggregate>(events);
            return aggregate;
        }
    }
}