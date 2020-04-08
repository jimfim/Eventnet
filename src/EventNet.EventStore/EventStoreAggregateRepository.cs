using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventNet.Core;
using EventStore.ClientAPI;

namespace EventNet.EventStore
{
    public class EventStoreAggregateRepository<TAggregate> : IAggregateRepository<TAggregate> 
        where TAggregate : AggregateRoot
    {
        private readonly IEventStoreConnection _connection;
        private readonly IAggregateFactory _factory = new AggregateFactory();
        public EventStoreAggregateRepository(IEventStoreConnection connection)
        {
            _connection = connection;
        }
        
        public async Task SaveAsync(AggregateRoot aggregate)
        {
            var commitId = Guid.NewGuid();
            var events = aggregate.UncommittedEvents;
            if (events.Any() == false)
            {
                return;
            }
            
            var streamName = EventStoreExtensions.GetStreamName<TAggregate>(aggregate.AggregateId);
            var originalVersion = aggregate.Version;// - events.Count();
            var expectedVersion = originalVersion == 0 ? ExpectedVersion.NoStream : originalVersion - 1;
            var commitHeaders = new Dictionary<string, object>
            {
                {"CommitId", commitId},
                {"AggregateClrType", aggregate.GetType().AssemblyQualifiedName}
            };
            var eventsToSave = events.Select(e => e.ToEventData(commitHeaders)).ToList();
            await _connection.AppendToStreamAsync(streamName, expectedVersion, eventsToSave);
            aggregate.ClearUncommittedEvents();
        }
        
        public async Task<TAggregate> GetAsync(Guid id)
        {
            var events = new List<IAggregateEvent>();
            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;
            var streamName = EventStoreExtensions.GetStreamName<TAggregate>(id);
            do
            {
                currentSlice = await _connection
                    .ReadStreamEventsForwardAsync(streamName, nextSliceStart, 200, false);
                nextSliceStart = currentSlice.NextEventNumber;
                events.AddRange(currentSlice.Events.Select(x => (IAggregateEvent)x.DeserializeEvent()));
            } while (!currentSlice.IsEndOfStream);
            var aggregate = _factory.Create<TAggregate>(events);
            return aggregate;
        }
    }
}
