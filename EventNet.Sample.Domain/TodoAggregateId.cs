using System;
using EventNet.Core;

namespace EventNet.Sample.Domain
{
    public class TodoAggregateId : IAggregateIdentity
    {
        public Guid Id { get; set; }

        public TodoAggregateId()
        {
            Id = new Guid();
        }

    }
}