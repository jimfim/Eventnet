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
    public class AggregateEventSubscriber<T>: IHostedService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IServiceProvider _provider;
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };
        
        public AggregateEventSubscriber(IConnectionMultiplexer connectionMultiplexer, IServiceProvider provider)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var streamName = RedisExtensions.GetStreamName<T>();
            var db = _connectionMultiplexer.GetDatabase();

            
            string nextSliceStart = "0-0";
            int batchSize = 100;
            var streams = new List<string>();
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                
                var endpoints = _connectionMultiplexer.GetEndPoints();
                foreach (var endpoint in endpoints)
                {
                    var server = _connectionMultiplexer.GetServer(endpoint);
                    var keys = server.KeysAsync();
                    var asyncEnumerator = keys.GetAsyncEnumerator(cancellationToken);
                    while (await asyncEnumerator.MoveNextAsync())
                    {
                        if (asyncEnumerator.Current.ToString().Contains(streamName))
                        {
                            streams.Add(asyncEnumerator.Current.ToString());
                        }
                    }
                }
                
                var currentSlice = await db.StreamReadAsync(streams.Select(s => new StreamPosition(s, nextSliceStart)).ToArray(),batchSize);
                foreach (var streamEntry in currentSlice)
                {
                    foreach (var entry in streamEntry.Entries)
                    {
                        foreach (var streamEntryValue in entry.Values)
                        {
                            var @event = JsonConvert.DeserializeObject<IAggregateEvent>(streamEntryValue.Value.ToString(), _serializerSettings);
                            await DispatchAsync(@event);
                            nextSliceStart  = entry.Id;        
                        }
                    }
                }
            }
        }


        private async Task DispatchAsync<TEvent>(TEvent @event) where TEvent : IAggregateEvent
        {
            var handlers = GetAllAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsInterface && !t.IsAbstract && t.GetInterfaces()
                    .Contains(typeof(IEventHandler<>).MakeGenericType(@event.GetType())))
                .Select(x => ActivatorUtilities.CreateInstance(_provider, x));

            foreach (var handler in handlers)
            {
                await HandlerRunnerAsync(handler, @event);
            }
        }

        private async Task HandlerRunnerAsync<TEvent>(object handler, TEvent @event) where TEvent : IAggregateEvent
        {
            handler.GetType().InvokeMember("HandleAsync", BindingFlags.InvokeMethod, null, handler,new object[] {@event});
            await Task.CompletedTask;
        }
        
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connectionMultiplexer.CloseAsync();
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