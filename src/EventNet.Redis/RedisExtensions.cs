using System;
using Newtonsoft.Json;

namespace EventNet.Redis
{
    public static class RedisExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
        
        public static string GetStreamIdKey()
        {
            return $"{Constants.NameSpace}-{Constants.PrimaryStream}-{Constants.MessageIdKey}";
        }
        
        public static string GetPrimaryStreamName()
        {
            return $"{Constants.NameSpace}-{Constants.PrimaryStream}";
        }
        
        public static string GetAggregateStreamCheckpoint<TEntity>()
        {
            return $"{Constants.NameSpace}-{typeof(TEntity)}-{Constants.CheckPoint}";
        }

        public static string GetAggregateStreamName<TEntity>(Guid id)
        {
            return $"{Constants.NameSpace}-{typeof(TEntity)}-{id}";
        }
        
        internal static string ToJson(this object input)
        {
            return JsonConvert.SerializeObject(input, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }

    public static class Constants
    {
        public static string NameSpace = "EventNet";
        public static string PrimaryStream = "Primary";
        public static string CheckPoint = "CheckPoint";
        public static string MessageIdKey = "MessageId";
    }
}