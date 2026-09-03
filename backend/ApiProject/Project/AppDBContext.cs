using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project
{
    public class AppDBContext: DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }
        public DbSet<Card> Card { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Donor> Donor { get; set; }
        public DbSet<Lottery> Lottery { get; set; }
        public DbSet<Present> Present { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Winner> Winner { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Donor>()
                .HasIndex(d => d.Email)
                .IsUnique();

            modelBuilder.Entity<Present>()
                .HasOne(p => p.Donor)
                .WithMany(d => d.Presents)
                .HasForeignKey(p => p.DonorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}