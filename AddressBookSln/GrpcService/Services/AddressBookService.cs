using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService.Protos;

namespace GrpcService.Services
{
    public class AddressBookService : Protos.AddressBookService.AddressBookServiceBase
    {
        private readonly ILogger<AddressBookService> _logger;
        private readonly AddressBook _addressBook;

        public AddressBookService(ILogger<AddressBookService> logger)
        {
            _logger = logger;

            // Create an initialize address book
            _addressBook = new AddressBook();
            _addressBook.Persons.Add(new Person { Name = "John Doe", Email = "john.doe@example.com" });
            _addressBook.Persons.Add(new Person { Name = "Mark Twain", Email = "mark.twain@example.com" });
            _addressBook.Persons.Add(new Person { Name = "Alice Smith", Email = "alice.smith@example.com" });
        }

        public override Task<AddressBook> GetAddressBook(Protos.Empty request, ServerCallContext context)
        {
            return Task.FromResult(_addressBook);
        }

        public override async Task FindPerson(FindPersonRequest request, IServerStreamWriter<Person> responseStream, ServerCallContext context)
        {
            foreach (var person in _addressBook.Persons)
            {
                // Compare based on FieldMask received values
                if (Matches(person, request.Person, request.FieldMask))
                {
                    await responseStream.WriteAsync(person);
                }
            }
        }

        private bool Matches(Person storedPerson, Person searchPerson, FieldMask fieldMask)
        {
            foreach (var path in fieldMask.Paths)
            {
                switch (path)
                {
                    case "name":
                        if (storedPerson.Name != searchPerson.Name)
                            return false;
                        break;
                    case "email":
                        if (storedPerson.Email != searchPerson.Email)
                            return false;
                        break;
                }
            }
            return true;
        }
    }
}
