namespace EventNet.Redis
{
    public static class Constants
    {
        public static readonly string NameSpace = "EventNet";
        public static readonly string PrimaryStream = "Primary";
        public static readonly string StreamData = "Data";
        public static readonly string CheckPoint = "CheckPoint";
        public static readonly string MessageIdKey = "MessageId";
        public static readonly string Delimiter = ":";
    }
}