using GrpcService.Models;
using GrpcService.Services;
using Serilog;

namespace GrpcService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog for structured logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            builder.Host.UseSerilog();

            // Load appsettings.json configuration file
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Register EF Core with SQLite as db provider
            builder.Services.AddDbContext<AddressBookContext>();

            // Add services to the container.
            builder.Services.AddGrpc();

            var app = builder.Build();

            // Apply database migrations
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AddressBookContext>();
                dbContext.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            //app.MapGrpcService<GreeterService>();

            app.MapGrpcService<AddressBookService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.UseRouting();

            app.Run();
        }
    }
}