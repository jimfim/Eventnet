using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EventNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EventNet.Redis.Subscriptions
{
    public class AggregateEventSubscriber<T> : IHostedService
    {
        private readonly ICheckPoint _checkPoint;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IServiceProvider _provider;
        private Dictionary<string,IEnumerable<object>> _handlers;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public AggregateEventSubscriber(IConnectionMultiplexer connectionMultiplexer, IServiceProvider provider,
            ICheckPoint checkPoint)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _provider = provider;
            _checkPoint = checkPoint;
            _handlers = new Dictionary<string, IEnumerable<object>>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var streamName = RedisExtensions.GetPrimaryStreamName();
            var db = _connectionMultiplexer.GetDatabase();
            var batchSize = 1000;

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
                    continue;
                }
                
                var currentSlice = await db.StreamReadAsync(streamName, checkpoint, batchSize);
                
                foreach (var streamEntry in currentSlice)
                {
                    foreach (var streamEntryValue in streamEntry.Values)
                    {
                        var @event = JsonConvert.DeserializeObject<AggregateEvent>(streamEntryValue.Value.ToString(), _serializerSettings);
                        await DispatchAsync(@event);
                        await _checkPoint.SetCheckpoint<T>(streamEntry.Id);
                    }
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connectionMultiplexer.CloseAsync();
        }


        private async Task DispatchAsync<TEvent>(TEvent @event) where TEvent : AggregateEvent
        {
            if (!_handlers.ContainsKey(@event.GetType().ToString()))
            {
                var handlers = GetAllAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => !t.IsInterface && !t.IsAbstract && t.GetInterfaces()
                        .Contains(typeof(IEventHandler<>).MakeGenericType(@event.GetType())))
                    .Select(x => ActivatorUtilities.CreateInstance(_provider, x));
                _handlers.Add(@event.GetType().ToString(), handlers);
            }
            
            foreach (var handler in _handlers[@event.GetType().ToString()])
            {
                await HandlerRunnerAsync(handler, @event);
            }
        }

        private async Task HandlerRunnerAsync<TEvent>(object handler, TEvent @event) where TEvent : AggregateEvent
        {
            handler.GetType().InvokeMember("HandleAsync", BindingFlags.InvokeMethod, null, handler,
                new object[] {@event});
            await Task.CompletedTask;
        }

        private static Assembly[] GetAllAssemblies()
        {
            var assemblies = DependencyContext.Default.RuntimeLibraries
                .Where(x => x.Name.StartsWith("EventNet"))
                .Where(lib => lib.RuntimeAssemblyGroups.Any())
                .Select(l => Assembly.Load(l.Name));
            return assemblies.ToArray();
        }
    }
}