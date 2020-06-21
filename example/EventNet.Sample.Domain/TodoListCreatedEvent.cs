using System;
using EventNet.Core;

namespace EventNet.Sample.Domain
{
    public class TodoListCreatedEvent : AggregateEvent
    {
        public TodoListCreatedEvent(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
        
        public string Name { get; }
    }
}