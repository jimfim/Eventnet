using System;

namespace EventNet.Core
{
    public interface IAggregateIdentity
    {
        Guid Id { get; set; }
    }
}