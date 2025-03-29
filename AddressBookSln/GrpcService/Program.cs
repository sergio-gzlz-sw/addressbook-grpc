using GrpcService.Models;
using GrpcService.Services;
using Serilog;

namespace GrpcService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog for structure logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting gRPC Server host...");

                // Create the host
                Host.CreateDefaultBuilder(args)
                    .UseSerilog()
                    .ConfigureServices((hostContext, services) =>
                    {
                        // Register BackgroundService hosting the gRPC server
                        services.AddHostedService<GrpcServerBackgroundService>();
                    })
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}