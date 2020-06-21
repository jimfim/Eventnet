using System;
using System.Collections.Generic;
using System.Text;
using EventNet.Core;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventNet.EventStore
{
    public static class EventStoreExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
        public static EventData ToEventData(this AggregateEvent message, IDictionary<string, object> headers)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message, SerializerSettings));

            var eventHeaders = new Dictionary<string, object>(headers)
            {
                {
                    EventMetaDataKeys.EventClrType, message.GetType().AssemblyQualifiedName
                }
            };
            
            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, SerializerSettings));
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
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(@event.OriginalEvent.Metadata)).Property(EventMetaDataKeys.EventClrType).Value;
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(@event.OriginalEvent.Data), Type.GetType((string)eventClrTypeName));
        }


    }
}