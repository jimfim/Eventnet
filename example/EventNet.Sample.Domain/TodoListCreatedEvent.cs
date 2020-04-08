using System;
using EventNet.Core;

namespace EventNet.Sample.Domain
{
    public class TodoListCreatedEvent : IAggregateEvent
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
    }
}