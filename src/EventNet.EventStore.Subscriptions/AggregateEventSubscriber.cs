using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventNet.Core;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventNet.EventStore.Subscriptions
{
    public class AggregateEventSubscriber<T>: IHostedService
    {
        private readonly IServiceProvider _provider;
        private IEventStoreConnection _eventStoreConnection;

        public AggregateEventSubscriber(IServiceProvider provider)
        {
            _provider = provider;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            int DEFAULT_PORT = 1113;
            UserCredentials credentials = new UserCredentials("admin", "changeit");
            
            var settings = ConnectionSettings.Create()
                .SetDefaultUserCredentials(credentials)
                .UseConsoleLogger()
                .Build();
            
            //var eventStoreConnection = EventStoreConnection.Create(settings, new IPEndPoint(IPAddress.Loopback, DEFAULT_PORT));
            _eventStoreConnection = EventStoreConnection.Create(settings, new IPEndPoint(IPAddress.Loopback, DEFAULT_PORT));
            await _eventStoreConnection.ConnectAsync();
            var catchUpSettings = new CatchUpSubscriptionSettings(100, 100, true, true, "testSubscriptionName");
            var stream = $"$ce-{typeof(T).FullName}";
            _eventStoreConnection.SubscribeToStreamFrom(stream, 0, catchUpSettings, EventAppeared);
            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _eventStoreConnection.Close();
            return Task.CompletedTask;
        }
        
        private async Task EventAppeared(EventStoreCatchUpSubscription eventStoreCatchUpSubscription,
            ResolvedEvent resolvedEvent)
        {
            var data = Encoding.ASCII.GetString(resolvedEvent.Event.Data);
            var metadata = JObject.Parse(Encoding.UTF8.GetString(resolvedEvent.Event.Metadata));
            var eventType = metadata.Property(EventMetaDataKeys.EventClrType).Value.ToString();
            var type = Type.GetType(eventType, AssemblyResolver, null);
            var response = (IAggregateEvent) JsonConvert.DeserializeObject(data, type);
            await DispatchAsync(response);
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