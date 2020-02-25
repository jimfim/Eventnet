using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventNet.Core
{
    public interface IAggregateRepository<TAggregateRoot, TAggregateRootId> where TAggregateRoot : AggregateRoot
        where TAggregateRootId : IAggregateIdentity
    {
        Task SaveAsync(IEnumerable<IAggregateEvent> events);

        Task<IEnumerable<IAggregateEvent>> GetAsync();

    }
}
