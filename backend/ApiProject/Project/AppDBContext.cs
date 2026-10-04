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
        public DbSet<Role> Role { get; set; }
        public DbSet<Winner> Winner { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasOne(user => user.Role)
                .WithMany()
                .HasForeignKey(user => user.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>().HasQueryFilter(role => !role.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(user => !user.IsDeleted);
            modelBuilder.Entity<Card>().HasQueryFilter(card => !card.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(category => !category.IsDeleted);
            modelBuilder.Entity<Lottery>().HasQueryFilter(lottery => !lottery.IsDeleted);
            modelBuilder.Entity<Present>().HasQueryFilter(present => !present.IsDeleted);
            modelBuilder.Entity<Winner>().HasQueryFilter(winner => !winner.IsDeleted);
        }
    }
}