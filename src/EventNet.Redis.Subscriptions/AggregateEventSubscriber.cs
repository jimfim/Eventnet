using System;
using System.Threading;
using System.Threading.Tasks;
using EventNet.Core;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Redis.Stream.Subscriber;

namespace EventNet.Redis.Subscriptions
{
    public class AggregateEventSubscriber<T> : IHostedService
    {
        private readonly ICheckPoint _checkPoint;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IRedisStreamClient _redisStreamClient;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public AggregateEventSubscriber(IServiceProvider provider,
            ICheckPoint checkPoint, IRedisStreamClient redisStreamClient)
        {
            _checkPoint = checkPoint;
            _redisStreamClient = redisStreamClient;
            _eventDispatcher = new EventDispatcher(provider);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var streamName = StreamNameExtensions.GetPrimaryStream();
            var checkpoint = await _checkPoint.GetCheckpoint<T>();
            var entries = _redisStreamClient.ReadStreamAsync(streamName, checkpoint, cancellationToken);
            await foreach (var entry in entries.WithCancellation(cancellationToken)) await EventAppeared(entry);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _redisStreamClient.Close();
        }

        private async Task EventAppeared(StreamEntry arg)
        {
            var @event = JsonConvert.DeserializeObject<AggregateEvent>(arg.Data, _serializerSettings);
            await _eventDispatcher.DispatchAsync(@event);
            await _checkPoint.SetCheckpoint<T>(arg.Id);
        }
    }
}