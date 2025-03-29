using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService.Models;
using GrpcService.Protos;
using Person = GrpcService.Models.Person;
using Serilog;

namespace GrpcService.Services
{
    public class AddressBookService : Protos.AddressBookService.AddressBookServiceBase
    {
        private readonly ILogger<AddressBookService> _logger;
        //private readonly AddressBook _addressBook;
        private readonly AddressBookContext _dbContext;

        public AddressBookService(ILogger<AddressBookService> logger, AddressBookContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

            // Seed database if empty
            if (!_dbContext.Persons.Any())
            {
                _dbContext.Persons.AddRange(new List<Person>
            {
                new Person { Name = "John Doe", Email = "john.doe@example.com" },
                new Person { Name = "Jane Doe", Email = "jane.doe@example.com" },
                new Person { Name = "Alice Smith", Email = "alice.smith@example.com" },
                new Person { Name = "Mark Twain", Email = "mark.twain@example.com" },
                new Person { Name = "Mark Twain Junior", Email = "mark.twain@example.com" }
            });
                _dbContext.SaveChanges();
                Log.Information("Database seeded with initial AddressBook data.");
            }
        }

        public override async Task<AddressBook> GetAddressBook(Protos.Empty request, ServerCallContext context)
        {
            Log.Information("Received GetAddressBook request.");

            var persons = await Task.FromResult(_dbContext.Persons.ToList());

            Log.Information("Returning {PersonCount} persons.", persons.Count);
            var response = new AddressBook();
            response.Persons.AddRange(persons.Select(p => new Protos.Person
            {
                Name = p.Name,
                Email = p.Email
            }));

            return response;
        }

        public override async Task FindPerson(FindPersonRequest request, IServerStreamWriter<Protos.Person> responseStream, ServerCallContext context)
        {
            Log.Information("Received FindPerson request with FieldMask: {FieldMask}", string.Join(", ", request.FieldMask.Paths));

            var query = _dbContext.Persons.AsQueryable();

            // Filter dabase query based on field mask
            foreach (var path in request.FieldMask.Paths)
            {
                switch (path)
                {
                    case "name":
                        query = query.Where(p => p.Name == request.Person.Name);
                        break;
                    case "email":
                        query = query.Where(p => p.Email == request.Person.Email);
                        break;
                }
            }

            var results = await Task.FromResult(query.ToList());

            Log.Information("Found {PersonCount} matching persons.", results.Count);

            foreach (var person in results)
            {
                await responseStream.WriteAsync(new Protos.Person
                {
                    Name = person.Name,
                    Email = person.Email
                });
            }
        }

    }
}
