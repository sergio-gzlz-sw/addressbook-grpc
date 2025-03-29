using Serilog;
using GrpcService.Models;

namespace GrpcService.Services
{
    public class GrpcServerBackgroundService : BackgroundService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private IHost? _webHost;

        public GrpcServerBackgroundService(IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Starting gRPC Background Service...");

            // Create a web host to run the gRPC server
            _webHost = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddGrpc();
                        services.AddDbContext<AddressBookContext>();
                    });

                    webBuilder.Configure(app =>
                    {
                        // Apply database migrations at startup
                        using var scope = app.ApplicationServices.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<AddressBookContext>();
                        dbContext.Database.EnsureCreated();

                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGrpcService<AddressBookService>();
                        });

                        Log.Information("gRPC Server is running...");
                    });
                })
                .Build();

            // Start the gRPC web host
            await _webHost.StartAsync(stoppingToken);

            // Stop the application gracefully when requested
            _appLifetime.ApplicationStopping.Register(async () =>
            {
                Log.Information("Stopping gRPC Service...");
                await _webHost.StopAsync();
            });
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Shutting down gRPC service Background Service...");
            if (_webHost != null)
            {
                await _webHost.StopAsync(cancellationToken);
                _webHost.Dispose();
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
