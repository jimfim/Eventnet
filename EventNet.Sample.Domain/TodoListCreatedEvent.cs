using EventNet.Core;

namespace EventNet.Sample.Domain
{
    public class TodoListCreatedEvent : IAggregateEvent
    {
        public TodoAggregateId AggregateId { get; }
        public string Name { get; }

        public TodoListCreatedEvent(TodoAggregateId aggregateId, string name)
        {
            AggregateId = aggregateId;
            Name = name;
        }
    }
}