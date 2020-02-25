using System;
using System.Collections.Generic;
using EventNet.Core;

namespace EventNet.Sample.Domain
{
    public class TodoAggregateRoot : AggregateRoot
    {
        public TodoAggregateRoot(string name)
        {
            AggregateId = new TodoAggregateId();
            var @event = new TodoListCreatedEvent(AggregateId, name);
            ApplyEvent(@event);
        }

        public TodoAggregateId AggregateId { get; set; }

        public string Name { get; set; }

        public List<TodoTask> Tasks { get; set; }

        public Guid AddTask(string text)
        {
            var newTodoId = Guid.NewGuid();
            var @event = new TodoTaskCreatedEvent(newTodoId, text);
            ApplyEvent(@event);
            return newTodoId;
        }

        protected void OnTodoListCreated(TodoListCreatedEvent @event)
        {
            AggregateId = @event.AggregateId;
            Name = @event.Name;
            Tasks = new List<TodoTask>();
        }

        protected void OnTodoTaskCreated(TodoTaskCreatedEvent @event)
        {
            var todo = new TodoTask(@event.Id, @event.Description);
            Tasks.Add(todo);
        }
    }
}