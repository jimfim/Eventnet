using System.Threading.Tasks;

namespace EventNet.Core
{
    public interface ICheckPoint
    {
        Task SetCheckpoint<TEntity>(string checkpoint);

        Task<uint> GetCheckpoint<TEntity>();
    }
}