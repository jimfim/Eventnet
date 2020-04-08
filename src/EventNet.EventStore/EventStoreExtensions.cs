using System;
using System.Collections.Generic;
using System.Text;
using EventNet.Core;
using EventStore.ClientAPI;
using System.Text.Json;

namespace EventNet.EventStore
{
    public static class EventStoreExtensions
    {
        public static EventData ToEventData(this IAggregateEvent message, IDictionary<string, object> headers)
        {
            var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var eventHeaders = new Dictionary<string, object>(headers)
            {
                {
                    EventMetaDataKeys.EventClrType, message.GetType().AssemblyQualifiedName
                }
            };
            
            var metadata = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventHeaders));
            var typeName = message.GetType().Name;
            var eventId = Guid.NewGuid();
            return new EventData(eventId, typeName, true, data, metadata);
        }
        
        public static string GetStreamName<TEntity>(Guid id)
        {
            return $"{typeof(TEntity)}-{id}";
        }
        
        public static object DeserializeEvent(this ResolvedEvent @event)
        {
            var metaData = JsonSerializer.Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(@event.OriginalEvent.Metadata));
            var clrType = metaData[EventMetaDataKeys.EventClrType].ToString();
            var data = Encoding.UTF8.GetString(@event.OriginalEvent.Data);
            return JsonSerializer.Deserialize(data,Type.GetType(clrType));;

        }
    }
}