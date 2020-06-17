using System;
using System.Threading.Tasks;
using EventNet.Core;
using EventNet.Sample.Domain;

namespace EventNet.Sample.ReadModel.EventsHandlers
{
    public class TodoListCreatedEventHandler : IEventHandler<TodoListCreatedEvent>
    {
        public async void HandleAsync(TodoListCreatedEvent @event)
        {
            Console.WriteLine($"event received : {@event.Id} {@event.Name}");
            await Task.CompletedTask;
        }
    }
}