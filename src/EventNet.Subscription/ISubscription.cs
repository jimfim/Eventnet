using System;
using EventNet.Core;

namespace EventNet.Subscription
{
    public interface ISubscription
    {
        IObservable<AggregateEvent> Subscribe { get; }
    }
}