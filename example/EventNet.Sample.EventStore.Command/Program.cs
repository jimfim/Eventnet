using System;
using System.Net;
using System.Threading.Tasks;
using EventNet.Core;
using EventNet.EventStore;
using EventNet.Sample.Domain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;

namespace EventNet.Sample.EventStore.Command
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int DEFAULT_PORT = 1113;
            UserCredentials credentials = new UserCredentials("admin", "changeit");
            
            var settings = ConnectionSettings.Create()
                .SetDefaultUserCredentials(credentials)
                .UseConsoleLogger()
                .Build();
            
            var eventStoreConnection = EventStoreConnection.Create(settings, new IPEndPoint(IPAddress.Loopback, DEFAULT_PORT));
            await eventStoreConnection.ConnectAsync();
            var eventStoreAggregateRepository = new EventStoreAggregateRepository<TodoAggregateRoot>(eventStoreConnection);
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
            Console.WriteLine(JsonConvert.SerializeObject(aggregate,Formatting.Indented));
        }
    }
}