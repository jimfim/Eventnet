using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace EventNet.Redis.Subscriptions
{
    internal interface IEventDispatcher
    {
        Task DispatchAsync<TEvent>(TEvent @event) where TEvent : AggregateEvent;
    }

    internal class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _provider;
        private readonly Dictionary<string,IEnumerable<object>> _handlers;

        public EventDispatcher(IServiceProvider provider)
        {
            _provider = provider;
            _handlers = new Dictionary<string, IEnumerable<object>>();
        }

        private async Task HandlerRunnerAsync<TEvent>(object handler, TEvent @event) where TEvent : AggregateEvent
        {
            handler.GetType().InvokeMember("HandleAsync", BindingFlags.InvokeMethod, null, handler,
                new object[] {@event});
            await Task.CompletedTask;
        }

        public async Task DispatchAsync<TEvent>(TEvent @event) where TEvent : AggregateEvent
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