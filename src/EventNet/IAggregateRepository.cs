using System;
using System.Threading.Tasks;

namespace EventNet.Core
{
    public interface IAggregateRepository<TEntity>
        where TEntity : AggregateRoot
    {
        Task SaveAsync(AggregateRoot aggregate);

        Task<TEntity> GetAsync(Guid id);
    }
}
