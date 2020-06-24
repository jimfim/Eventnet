using System;
using System.Collections.Generic;

namespace EventNet.Core
{
    public abstract class AggregateRoot
    {
        private readonly List<AggregateEvent> _uncommittedEvents = new List<AggregateEvent>();

        public int Version { get; private set; }

        public Guid AggregateId { get; protected set; }

        public List<AggregateEvent> UncommittedEvents => _uncommittedEvents;
        
        protected void Publish(AggregateEvent @event)
        {
            @event.AggregateId = this.AggregateId;
            UncommittedEvents.Add(@event);
        }
        
        private void Apply(object e)
        {
            Version++;
            RedirectToWhen.InvokeEventOptional(this, e);
        }

        public void MarkChangesAsCommitted()
        {
            _uncommittedEvents.Clear();
        }

        public void LoadsFromHistory(IEnumerable<AggregateEvent> events)
        {
            foreach (var e in events)
            {
                Apply(e);
            }
        }
    }
}