using Microsoft.EntityFrameworkCore;
using TruckScalesWeb.Models;

namespace TruckScalesWeb.DAO
{
    public class SqlContext:DbContext
    {
        public DbSet<Car> Cars { get; set; }

        public SqlContext(DbContextOptions<SqlContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
