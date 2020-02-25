using EventNet.Sample.Domain;
using System;
using EventNet.Core;
using EventNet.Eventstore;

namespace Eventnet.Sample.Command
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new EventStoreAggregateRepository<TodoAggregateRoot, TodoAggregateId>();
            var agg = repo.Get(new TodoAggregateId());
            agg.AddTask("testing");
            repo.Save(agg);
        }
    }
}