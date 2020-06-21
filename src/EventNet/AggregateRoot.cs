using System;
using System.Collections.Generic;

namespace EventNet.Core
{
    public abstract class AggregateRoot
    {
        private int _version;
        private readonly List<AggregateEvent> _uncommittedEvents = new List<AggregateEvent>();

        public int Version => _version;

        public Guid AggregateId { get; protected set; }

        public List<AggregateEvent> UncommittedEvents => _uncommittedEvents;
        
        protected AggregateRoot(Guid id)
        {
            AggregateId = id;
        }

        protected  AggregateRoot(IEnumerable<AggregateEvent> events)
        {
            foreach (var e in events)
            {
                Apply(e);
            }
        }
        
        protected void Publish(AggregateEvent @event)
        {
            @event.AggregateId = this.AggregateId;
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