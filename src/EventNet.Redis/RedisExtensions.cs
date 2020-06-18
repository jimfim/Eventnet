using System;
using Newtonsoft.Json;

namespace EventNet.Redis
{
    public static class RedisExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
        
        public static string GetStreamName<TEntity>()
        {
            return $"{typeof(TEntity)}";
        }
        
        public static string GetAggregateStreamName<TEntity>(Guid id)
        {
            return $"{typeof(TEntity)}-{id}";
        }
        
        internal static string ToJson(this object input)
        {
            return JsonConvert.SerializeObject(input, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }
}