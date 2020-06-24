using System;
using System.Threading;
using System.Threading.Tasks;
using EventNet.Core;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EventNet.Redis.Subscriptions
{
    public class AggregateEventSubscriber<T> : IHostedService
    {
        private readonly ICheckPoint _checkPoint;
        private readonly Configuration _configuration;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public AggregateEventSubscriber(IConnectionMultiplexer connectionMultiplexer, IServiceProvider provider,
            ICheckPoint checkPoint, Configuration configuration = null)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _checkPoint = checkPoint;
            _configuration = configuration ?? new Configuration();
            _eventDispatcher = new EventDispatcher(provider);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var streamName = RedisExtensions.GetPrimaryStream();
            IDatabase db = _connectionMultiplexer.GetDatabase();
            var batchSize = _configuration.BatchSize;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                var checkpoint = await _checkPoint.GetCheckpoint<T>();
                var streamInfo = db.StreamInfo(streamName);
                if (streamInfo.LastEntry.Id == checkpoint)
                {
                    await Task.Delay(_configuration.Delay, cancellationToken);
                    continue;
                }
                
                var currentSlice = await db.StreamReadAsync(streamName, checkpoint, batchSize);
                
                foreach (var streamEntry in currentSlice)
                {
                    foreach (var streamEntryValue in streamEntry.Values)
                    {
                        var @event = JsonConvert.DeserializeObject<AggregateEvent>(streamEntryValue.Value.ToString(), _serializerSettings);
                        await _eventDispatcher.DispatchAsync(@event);
                        await _checkPoint.SetCheckpoint<T>(streamEntry.Id);
                    }
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connectionMultiplexer.CloseAsync();
        }
    }
}