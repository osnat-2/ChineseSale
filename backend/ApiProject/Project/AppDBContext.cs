using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project
{
    public class AppDBContext: DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }
        public DbSet<Card> Card { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Lottery> Lottery { get; set; }
        public DbSet<Present> Present { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Winner> Winner { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}