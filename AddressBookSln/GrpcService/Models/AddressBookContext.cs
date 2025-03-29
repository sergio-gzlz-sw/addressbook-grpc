using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GrpcService.Models
{
    public class AddressBookContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }

        private readonly string _connectionString;

        public AddressBookContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AddressBookDatabase");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite(_connectionString);
        }
    }
}
