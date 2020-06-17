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
            
            var streamName = RedisExtensions.GetStreamName<TAggregate>();
            foreach (var @event in events)
            {
                await db.StreamAddAsync(streamName, aggregate.AggregateId.ToString(), @event.ToJson());    
            }
            
            aggregate.ClearUncommittedEvents();
        }

        public async Task<TAggregate> GetAsync(Guid id)
        {
            var streamName = RedisExtensions.GetStreamName<TAggregate>();
            var db = _connectionMultiplexer.GetDatabase();

            var events = new List<IAggregateEvent>();
            long nextSliceStart = 0;
            int batchSize = 100;
            var info = db.StreamInfo(streamName);
            
            do
            {
                var currentSlice = await db.StreamReadAsync(streamName, nextSliceStart, batchSize);
                nextSliceStart +=batchSize;
                foreach (var streamEntry in currentSlice)
                {
                    foreach (var streamEntryValue in streamEntry.Values)
                    {
                        if (streamEntryValue.Name.ToString() == id.ToString())
                        {
                            events.Add(JsonConvert.DeserializeObject<IAggregateEvent>(streamEntryValue.Value.ToString(),new JsonSerializerSettings()
                            {
                                TypeNameHandling = TypeNameHandling.All
                            }));    
                        }
                    }
                }
            } while (nextSliceStart < info.Length);
            var aggregate = _factory.Create<TAggregate>(events);
            return aggregate;
        }
    }
}