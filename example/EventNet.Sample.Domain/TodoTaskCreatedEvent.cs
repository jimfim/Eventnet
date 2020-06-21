using System;
using EventNet.Core;

namespace EventNet.Sample.Domain
{
    public class TodoTaskCreatedEvent : AggregateEvent
    {
        public string Description { get; }

        public TodoTaskCreatedEvent(Guid id, string description)
        {
            Id = id;
            Description = description;
        }
    }
}