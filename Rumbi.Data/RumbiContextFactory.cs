using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Rumbi.Data.Config;

namespace Rumbi.Data
{
    //For applying migrations/scaffolding
    public class RumbiContextFactory : IDesignTimeDbContextFactory<RumbiContext>
    {
        public RumbiContext CreateDbContext(string[] args)
        {
            var config = new RumbiConfig();
            var optionsBuilder = new DbContextOptionsBuilder<RumbiContext>().UseNpgsql(
                config.ConnectionString
            );

            return new RumbiContext(optionsBuilder.Options);
        }
    }
}
