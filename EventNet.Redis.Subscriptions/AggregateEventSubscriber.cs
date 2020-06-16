using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
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

        public AggregateEventSubscriber(IConnectionMultiplexer connectionMultiplexer,IServiceProvider provider)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _provider = provider;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var db = _connectionMultiplexer.GetDatabase();
            RedisValue position = 0;
            var stream = $"{typeof(T).FullName}";
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                var result = db.StreamRead(stream, position, 1);
                position = result[0].Id;
                Console.WriteLine(result[0].Values[0]);
                var @event = (IAggregateEvent)JsonConvert.DeserializeObject(result[0].Values[0].Value, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                await DispatchAsync(@event);
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