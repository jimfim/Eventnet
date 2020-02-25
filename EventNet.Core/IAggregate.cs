using System;

namespace EventNet.Core
{
    public abstract class AggregateRoot : AggregateRoot<Guid,Guid>
    {

    }

    public abstract class AggregateRoot<TAggregateKey, TEventKey>
    {
        protected void ApplyEvent(IAggregateEvent @event)
        {

        }
    }
}