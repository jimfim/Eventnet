namespace EventNet.Core
{
    public interface IAggregateRepository<TAggregateRoot, TAggregateRootId> where TAggregateRoot : AggregateRoot
        where TAggregateRootId : IAggregateIdentity
    {
        TAggregateRoot Get(TAggregateRootId id);

        void Save(TAggregateRoot aggregateRoot);
    }
}
