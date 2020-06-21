using System;
using System.Collections.Generic;

namespace EventNet.Core
{
    public interface IAggregateFactory
    {
        T Create<T>(IEnumerable<object> events) where T : AggregateRoot;
        
        T Create<T>(Guid aggregateId) where T : AggregateRoot;
    }

    public class AggregateFactory : IAggregateFactory
    {
        public TAggregate Create<TAggregate>(IEnumerable<object> events) where TAggregate : AggregateRoot
        {
            var aggregate = (TAggregate) Activator.CreateInstance(typeof(TAggregate), events ?? new List<AggregateEvent>());
            return aggregate;

        }

        public TAggregate Create<TAggregate>(Guid aggregateId) where TAggregate : AggregateRoot
        {
            var aggregate = (TAggregate) Activator.CreateInstance(typeof(TAggregate), aggregateId);
            return aggregate;
        }
    }
}