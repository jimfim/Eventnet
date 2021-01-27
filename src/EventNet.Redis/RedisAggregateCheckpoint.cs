using System.Linq;
using System.Threading.Tasks;
using EventNet.Core;
using StackExchange.Redis;

namespace EventNet.Redis
{
    public class RedisAggregateCheckpoint : ICheckPoint
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisAggregateCheckpoint(IConnectionMultiplexer multiplexer)
        {
            _connectionMultiplexer = multiplexer;
        }
        
        public async Task SetCheckpoint<TEntity>(string checkpoint)
        {
            var id = checkpoint.Split("-").First();
            var db = _connectionMultiplexer.GetDatabase();
            var checkpointKey = StreamNameExtensions.GetAggregateStreamCheckpoint<TEntity>();
            await db.StringSetAsync(checkpointKey, $"{id}");
        }

        public async Task<uint> GetCheckpoint<TEntity>()
        {
            var db = _connectionMultiplexer.GetDatabase();
            var checkpointKey = StreamNameExtensions.GetAggregateStreamCheckpoint<TEntity>();
            var checkpoint = await db.StringGetAsync(checkpointKey);
            if (!checkpoint.HasValue)
            {
                return (uint) await Task.FromResult(0); 
            }
            return (uint) await Task.FromResult(checkpoint);   
        }
    }
}