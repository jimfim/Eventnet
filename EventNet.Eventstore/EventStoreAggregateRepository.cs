using System;
using EventNet.Core;

namespace EventNet.Eventstore
{
    public class EventStoreAggregateRepository<TAggregateRoot, TAggregateRootId> : IAggregateRepository<TAggregateRoot, TAggregateRootId> 
        where TAggregateRoot : AggregateRoot 
        where TAggregateRootId : IAggregateIdentity
    {
        public TAggregateRoot Get(TAggregateRootId aggregateIdentity)
        {
            throw new NotImplementedException();
        }

        public void Save(TAggregateRoot aggregateRoot)
        {
            throw new NotImplementedException();
        }
    }
}
