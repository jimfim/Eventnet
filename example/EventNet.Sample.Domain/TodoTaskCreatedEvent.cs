using System;
using EventNet.Core;

namespace EventNet.Sample.Domain
{
    public class TodoTaskCreatedEvent : IAggregateEvent
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
    }
}