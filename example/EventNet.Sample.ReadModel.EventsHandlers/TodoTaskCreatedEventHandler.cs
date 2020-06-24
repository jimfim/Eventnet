using System;
using System.Threading.Tasks;
using EventNet.Core;
using EventNet.Sample.Domain;
using EventNet.Sample.ReadModel.Repository;

namespace EventNet.Sample.ReadModel.EventsHandlers
{
    public class TodoTaskCreatedEventHandler : IEventHandler<TodoTaskCreatedEvent>
    {
        private readonly ITodoRepository _repository;

        public TodoTaskCreatedEventHandler(ITodoRepository repository)
        {
            _repository = repository;
        }
        
        public async void HandleAsync(TodoTaskCreatedEvent @event)
        {
            var model = await _repository.GetAsync(@event.AggregateId);
            if (!model.Tasks.ContainsKey(@event.Id))
            {
                model.Tasks.Add(@event.Id, @event.Description);
            }
            else
            {
                model.Tasks[@event.Id] = @event.Description;
            }

            await _repository.SaveAsync(model);
            await Console.Out.WriteLineAsync($"event received : {@event.Id} {@event.Description}");
        }
    }
}