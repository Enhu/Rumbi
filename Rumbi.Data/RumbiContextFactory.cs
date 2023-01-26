using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Rumbi.Data.Config;

namespace Rumbi.Data
{
    public class RumbiContextFactory : IDesignTimeDbContextFactory<RumbiContext>
    {
        public RumbiContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RumbiContext>()
                .UseNpgsql(RumbiConfig.Configuration.ConnectionString);

            return new RumbiContext(optionsBuilder.Options);
        }
    }
}
