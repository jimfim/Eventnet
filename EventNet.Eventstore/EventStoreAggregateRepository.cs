using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventNet.Core;
using EventStore.ClientAPI;

namespace EventNet.Eventstore
{
    public class EventStoreAggregateRepository<TAggregateRoot, TAggregateRootId> : IAggregateRepository<TAggregateRoot, TAggregateRootId> 
        where TAggregateRoot : AggregateRoot 
        where TAggregateRootId : IAggregateIdentity
    {
        private readonly IEventStoreConnection _connection;

        internal EventStoreAggregateRepository(IEventStoreConnection connection)
        {
            _connection = connection;
        }
        public Task SaveAsync(IEnumerable<IAggregateEvent> events)
        {
            _connection.AppendToStreamAsync("somestream", ExpectedVersion.Any, events.Select( x => x.ToEventData())).ConfigureAwait(false);
        }

        public Task<IEnumerable<IAggregateEvent>> GetAsync()
        {
            throw new NotImplementedException();
        }
    }

    public static class EventStoreExtensions
    {
        public static EventData ToEventData(this IAggregateEvent message)
        {
            var data = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));
            //var metadata = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(headers));
            var typeName = message.GetType().Name;
            return new EventData(message.Id, typeName, true, data, null);
        }
    }
}
