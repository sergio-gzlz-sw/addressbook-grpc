using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcAddressBookClient.Protos;
using static GrpcAddressBookClient.Protos.AddressBookService;

namespace GrpcAddressBookClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:7271");
            AddressBookServiceClient client = new AddressBookServiceClient(channel);

            Console.WriteLine("Fetch AddressBook from gRPC Server...");
            var addressbook = client.GetAddressBook(new Protos.Empty());

            Console.WriteLine("\nAddressBook Entries:");
            foreach (var person in addressbook.Persons)
            {
                Console.WriteLine($"- {person.Name}, {person.Email}");
            }

            Console.WriteLine("\n\nFinding persons with email address 'mark.twain@example.com'...\n");
            using var call = client.FindPerson(new FindPersonRequest
            {
                Person = new Person { Email = "mark.twain@example.com" },
                FieldMask = new FieldMask { Paths = { "email" } }
            });

            await foreach (var person in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"Found: {person.Name}, {person.Email}");
            }

            Console.WriteLine("\nPress any enter to exit...");
            Console.ReadLine();
        }
    }
}