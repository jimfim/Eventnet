using System;

namespace EventNet.Core
{
    public abstract class AggregateEvent
    {
        public Guid AggregateId { get; set; }
        
        public Guid Id { get; set; }
    }
}