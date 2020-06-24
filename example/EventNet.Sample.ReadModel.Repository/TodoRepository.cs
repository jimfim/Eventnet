using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EventNet.Sample.ReadModel.Repository
{
    public class TodoRepository : ITodoRepository
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public TodoRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }
        
        public async Task<TodoViewModel> GetAsync(Guid id)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var data = await db.StringGetAsync($"ReadModel:Todo:{id}");
            return JsonConvert.DeserializeObject<TodoViewModel>(data);
        }

        public async Task SaveAsync(TodoViewModel model)
        {
            var db = _connectionMultiplexer.GetDatabase();
            await db.StringSetAsync($"ReadModel:Todo:{model.Id}",JsonConvert.SerializeObject(model));
        }
    }
}