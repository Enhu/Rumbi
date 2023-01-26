using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Rumbi.Data
{
    public class RumbiContextFactory : IDesignTimeDbContextFactory<RumbiContext>
    {
        public RumbiContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RumbiContext>()
                .UseNpgsql("Server=127.0.0.1;Port=5432;Databse=RumbiDB;User Id=postgres;Password=admin");

            return new RumbiContext(optionsBuilder.Options);
        }
    }
}
