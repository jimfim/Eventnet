using System;
using System.Threading.Tasks;
using EventNet.Core;
using EventNet.Sample.Domain;
using EventNet.Sample.ReadModel.Repository;

namespace EventNet.Sample.ReadModel.EventsHandlers
{
    public class TodoListCreatedEventHandler : IEventHandler<TodoListCreatedEvent>
    {
        private readonly ITodoRepository _repository;

        public TodoListCreatedEventHandler(ITodoRepository repository)
        {
            _repository = repository;
        }
        public async void HandleAsync(TodoListCreatedEvent @event)
        {
            await _repository.SaveAsync(new TodoViewModel()
            {
                Id = @event.Id,
                Name = @event.Name
            });
            await Console.Out.WriteLineAsync($"event received : {@event.AggregateId} {@event.Name}");
        }
    }
}