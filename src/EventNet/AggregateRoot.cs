using System;
using System.Collections.Generic;

namespace EventNet.Core
{
    public abstract class AggregateRoot
    {
        private int _version;
        private readonly List<IAggregateEvent> _uncommittedEvents = new List<IAggregateEvent>();

        public int Version => _version;

        public Guid AggregateId { get; protected set; }

        public List<IAggregateEvent> UncommittedEvents => _uncommittedEvents;
        
        protected AggregateRoot(Guid id)
        {
            AggregateId = id;
        }

        protected  AggregateRoot(IEnumerable<IAggregateEvent> events)
        {
            foreach (var e in events)
            {
                Apply(e);
            }
        }
        
        protected void Publish(IAggregateEvent @event)
        {
            UncommittedEvents.Add(@event);
        }
        
        private void Apply(object e)
        {
            _version++;
            RedirectToWhen.InvokeEventOptional(this, e);
        }

        public void MarkEventsCompleted()
        {
            _uncommittedEvents.Clear();
        }
    }
}