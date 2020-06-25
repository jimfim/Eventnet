using System;
using Newtonsoft.Json;

namespace EventNet.Redis
{
    public static class RedisExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
        
        public static string GetStreamIdKey()
        {
            return $"{Constants.NameSpace}{Constants.Delimiter}{Constants.PrimaryStream}-{Constants.MessageIdKey}";
        }
        
        public static string GetPrimaryStream()
        {
            return $"{Constants.NameSpace}{Constants.Delimiter}{Constants.PrimaryStream}";
        }
        
        public static string GetAggregateStreamCheckpoint<TEntity>()
        {
            return $"{Constants.NameSpace}{Constants.Delimiter}{typeof(TEntity)}-{Constants.CheckPoint}";
        }

        public static string GetAggregateStream<TEntity>(Guid id)
        {
            return $"{Constants.NameSpace}{Constants.Delimiter}{typeof(TEntity)}{Constants.Delimiter}{id}";
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