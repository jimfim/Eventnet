using System;
using System.Collections.Generic;

namespace EventNet.Core
{
    public interface IAggregateFactory
    {
        T Create<T>(IEnumerable<AggregateEvent> events) where T : AggregateRoot;
        
        T Create<T>() where T : AggregateRoot;
    }

    public class AggregateFactory : IAggregateFactory
    {
        public TAggregate Create<TAggregate>(IEnumerable<AggregateEvent> events) where TAggregate : AggregateRoot
        {
            var aggregate = (TAggregate) Activator.CreateInstance(typeof(TAggregate));
            aggregate.LoadsFromHistory(events);
            return aggregate;
        }

        public TAggregate Create<TAggregate>() where TAggregate : AggregateRoot
        {
            var aggregate = (TAggregate) Activator.CreateInstance(typeof(TAggregate));
            return aggregate;
        }
    }
}