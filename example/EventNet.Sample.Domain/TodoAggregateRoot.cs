using System;
using System.Collections.Generic;
using EventNet.Core;

namespace EventNet.Sample.Domain
{
    public class TodoAggregateRoot : AggregateRoot
    {
        public string Name { get; private set; }

        public List<TodoTask> Tasks { get; private set; }

        public void Create(Guid id, string name)
        {
            if (Version > 0) throw new InvalidOperationException("Cannot start a list more than once.");
            var @event = new TodoListCreatedEvent()
            {
                Id = id,
                Name = name
            };
            Publish(@event);
        }

        public void AddTask(Guid id, string text)
        {
            if (id == Guid.Empty) throw new InvalidOperationException("Id cannot be empty");
            if (string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("Description cannot be empty");

            var @event = new TodoTaskCreatedEvent()
            {
                Id = id,
                Description = text
            };
            Publish(@event);
        }

        public void When(TodoListCreatedEvent @event)
        {
            AggregateId = @event.Id;
            Name = @event.Name;
            Tasks = new List<TodoTask>();
        }

        public void When(TodoTaskCreatedEvent @event)
        {
            var todo = new TodoTask(@event.Id, @event.Description);
            Tasks.Add(todo);
        }

        public TodoAggregateRoot(Guid id) : base(id)
        {
        }

        public TodoAggregateRoot(IEnumerable<IAggregateEvent> events) : base(events)
        {
        }
    }
}