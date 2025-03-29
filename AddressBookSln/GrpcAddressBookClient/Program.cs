using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcAddressBookClient.Protos;
using static GrpcAddressBookClient.Protos.AddressBookService;
using Serilog;

namespace GrpcAddressBookClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting gRPC client...");

                using var channel = GrpcChannel.ForAddress("https://localhost:7271");
                AddressBookServiceClient client = new AddressBookServiceClient(channel);

                Log.Information("Fetching AddressBook from gRPC Server...");
                var addressbook = client.GetAddressBook(new Protos.Empty());

                Log.Information("Retrieved {PersonCount} persons from server.", addressbook.Persons.Count);
                foreach (var person in addressbook.Persons)
                {
                    Log.Information("Person: Name={Name}, Email={Email}", person.Name, person.Email);
                }

                Log.Information("Searching for persons whose email address is 'mark.twain@example.com'...");
                using var call = client.FindPerson(new FindPersonRequest
                {
                    Person = new Person { Email = "mark.twain@example.com" },
                    FieldMask = new FieldMask { Paths = { "email" } }
                });

                await foreach (var person in call.ResponseStream.ReadAllAsync())
                {
                    Log.Information("Found: Name={Name}, Email={Email}", person.Name, person.Email);
                }

                Log.Information("gRPC client execution completed successfully.");

                Console.WriteLine("\nPress enter to exit\n");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while executing the gRPC client.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}