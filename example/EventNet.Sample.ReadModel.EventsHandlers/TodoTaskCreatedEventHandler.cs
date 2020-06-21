using System;
using System.Threading.Tasks;
using EventNet.Core;
using EventNet.Sample.Domain;

namespace EventNet.Sample.ReadModel.EventsHandlers
{
    public class TodoTaskCreatedEventHandler : IEventHandler<TodoTaskCreatedEvent>
    {
        public async void HandleAsync(TodoTaskCreatedEvent @event)
        {
            await Console.Out.WriteLineAsync($"event received : {@event.Id} {@event.Description}");
        }
    }
}