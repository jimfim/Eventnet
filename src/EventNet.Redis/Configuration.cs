namespace EventNet.Redis
{
    public class Configuration
    {
        public int BatchSize { get; set; } = 100;
        public int Delay { get; set; } = 100;
        public bool ProjectionsEnabled { get; set; } = false;
    }
}