using System;
using EventNet.Core;

namespace EventNet.Sample.Domain
{
    public class TodoListCreatedEvent : IAggregateEvent
    {
        public TodoListCreatedEvent(Guid aggregateId, string name)
        {
            Id = aggregateId;
            Name = name;
        }
        
        public string Name { get; }
        public Guid Id { get; set; }
    }
}