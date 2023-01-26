using Microsoft.EntityFrameworkCore;
using Rumbi.Data.Models;

namespace Rumbi.Data
{
    public class RumbiContext : DbContext
    {
        public RumbiContext(DbContextOptions<RumbiContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.ApplyConfigurationsFromAssembly(typeof(RumbiContext).Assembly);

        public DbSet<Roles> Roles { get; set; } = null!;
    }
}
