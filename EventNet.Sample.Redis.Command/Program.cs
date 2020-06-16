using System;
using System.Threading.Tasks;
using EventNet.Core;
using EventNet.Redis;
using EventNet.Sample.Domain;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EventNet.Sample.Redis.Command
{
    internal class Program
    {
        internal static async Task Main()
        {
            var redis = await ConnectionMultiplexer.ConnectAsync("localhost");
            var eventStoreAggregateRepository = new RedisAggregateRepository<TodoAggregateRoot>(redis);
            IAggregateFactory factory = new AggregateFactory();

            var aggregateId = Guid.NewGuid();
            var agg = factory.Create<TodoAggregateRoot>(aggregateId);

            agg.Create(aggregateId, "My List");

            agg.AddTask(Guid.NewGuid(), "get 1 egg");
            agg.AddTask(Guid.NewGuid(), "get milk");
            agg.AddTask(Guid.NewGuid(), "dry clothes");
            agg.AddTask(Guid.NewGuid(), "solve world hunger");

            await eventStoreAggregateRepository.SaveAsync(agg);

            var aggregate = await eventStoreAggregateRepository.GetAsync(agg.AggregateId);
            Console.WriteLine(JsonConvert.SerializeObject(aggregate, Formatting.Indented));
        }
    }
}