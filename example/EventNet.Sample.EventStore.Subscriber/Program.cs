using EventNet.EventStore.Subscriptions;
using EventNet.Sample.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventNet.Sample.ReadModelSubscriber
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<AggregateEventSubscriber<TodoAggregateRoot>>();
                });
        }
    }
}