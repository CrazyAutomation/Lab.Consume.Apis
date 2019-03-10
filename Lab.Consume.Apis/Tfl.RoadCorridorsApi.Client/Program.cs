using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tfl.RoadCorridorsApi.Client.Config;
using Tfl.RoadCorridorsApi.Client.Models;
using Tfl.RoadCorridorsApi.Client.Services;
using static System.Environment;

namespace Tfl.RoadCorridorsApi.Client
{
    public class Program
    {
        public static IConfigurationRoot Configuration;

        private static async Task<int> Main(string[] args)
        {
            if (args.Length == 0)
                return ExitCode = 1;

            var roadId = args[0];

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(30000);
                var roadCorridorApiResponse = await serviceProvider.GetService<IRoadCorridorService>()
                                                    .GetRoadStatus(roadId, cancellationTokenSource.Token);
                return DisplayRoadStatus(roadId, roadCorridorApiResponse);
            }
            catch (Exception)
            {
                var logger = serviceProvider.GetService<ILogger<Program>>();
                logger.LogError("TFL Road Corridors: Failed to get road status.");
                return ExitCode = 1;
            }

        }

        private static int DisplayRoadStatus(string roadName, RoadCorridorResponse roadCorridorApiResponse)
        {
            switch (roadCorridorApiResponse.HttpStatus)
            {
                case HttpStatusCode.OK:
                    Console.WriteLine($"The status of the {roadCorridorApiResponse.RoadCorridor?.DisplayName} is as follows");
                    Console.WriteLine($"        Road status is {roadCorridorApiResponse.RoadCorridor?.RoadStatus}");
                    Console.WriteLine($"        Road status Description is {roadCorridorApiResponse.RoadCorridor?.RoadStatusDescription}");
                    return ExitCode = 0;
                case HttpStatusCode.NotFound:
                    Console.WriteLine($"{roadName} is not a valid road");
                    return ExitCode = 1;
                default:
                    Console.WriteLine($"Fail to get Road status for {roadName}");
                    return ExitCode = 1;
            }
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(new LoggerFactory()
                //.AddConsole()
                .AddDebug());
            serviceCollection.AddLogging();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
            serviceCollection.AddOptions();
            serviceCollection.Configure<TflApiSettings>(Configuration.GetSection("TflApiSettings"));
            serviceCollection.AddSingleton(Configuration);

            serviceCollection.AddHttpClient("TflRoadClient", httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://api.tfl.gov.uk/");
                httpClient.Timeout = new TimeSpan(0, 0, 45);
                httpClient.DefaultRequestHeaders.Clear();
            });

            serviceCollection.AddScoped<IRoadCorridorService, RoadCorridorService>();
        }
    }
}
