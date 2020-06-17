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
            int batchSize = 10;
            while (true)
            {
                var info = await db.StreamInfoAsync(streamName);
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (nextSliceStart == info.LastEntry.Id)
                {
                    continue;
                }
                
                var currentSlice =  await db.StreamReadAsync(streamName, nextSliceStart, batchSize);
                foreach (var streamEntry in currentSlice)
                {
                    foreach (var streamEntryValue in streamEntry.Values)
                    {
                        var @event = JsonConvert.DeserializeObject<IAggregateEvent>(streamEntryValue.Value.ToString(), _serializerSettings);
                        await DispatchAsync(@event);
                        nextSliceStart  = streamEntry.Id;        
                    }
                }
            }
        }
        
        
        public async Task DispatchAsync<TEvent>(TEvent @event) where TEvent : IAggregateEvent
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
        
        private static Assembly AssemblyResolver(AssemblyName arg)
        {
            return Assembly.Load(arg);
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