using System;

namespace EventNet.Core
{
    public interface IAggregateEvent
    {
        Guid Id { get; set; }
    }
}