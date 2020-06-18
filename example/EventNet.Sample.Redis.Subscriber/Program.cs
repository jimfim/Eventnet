using EventNet.Sample.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace EventNet.Sample.Redis.Subscriber
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));
                    services.AddHostedService<EventNet.Redis.Subscriptions.AggregateEventSubscriber<TodoAggregateRoot>>();
                });
        }
    }
}