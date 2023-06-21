using Microsoft.EntityFrameworkCore;
using Rumbi.Data.Models;

namespace Rumbi.Data
{
    public class RumbiContext : DbContext
    {
        public RumbiContext(DbContextOptions<RumbiContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Meme>().Property(p => p.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<AnnouncementAttachment>().Property(p => p.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<TemporalFile>().Property(p => p.Id).ValueGeneratedOnAdd();

            modelBuilder
                .Entity<Announcement>()
                .HasOne(a => a.Attachment)
                .WithOne(x => x.Announcement)
                .HasForeignKey<AnnouncementAttachment>(y => y.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Channel> Channels { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Meme> Memes { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; } = null!;
        public DbSet<AnnouncementAttachment> AnnouncementAttachments { get; set; } = null!;
        public DbSet<TemporalFile> TemporalFiles { get; set; } = null!;
        public DbSet<Poll> Polls { get; set; } = null!;
    }
}
