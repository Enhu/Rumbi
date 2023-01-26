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
        {
            modelBuilder.Entity<Meme>()
                  .Property(p => p.Id)
                  .ValueGeneratedOnAdd();
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Meme> Memes { get; set; } = null!;
    }
}
