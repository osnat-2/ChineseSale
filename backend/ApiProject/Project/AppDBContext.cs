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

            // modelBuilder.Entity<Role>().Ignore(role => role.CreatedByUser);
            // modelBuilder.Entity<User>().Ignore(user => user.CreatedByUser);
            // modelBuilder.Entity<Card>().Ignore(card => card.CreatedByUser);
            // modelBuilder.Entity<Category>().Ignore(category => category.CreatedByUser);
            // modelBuilder.Entity<Lottery>().Ignore(lottery => lottery.CreatedByUser);
            // modelBuilder.Entity<Present>().Ignore(present => present.CreatedByUser);
            // modelBuilder.Entity<Winner>().Ignore(winner => winner.CreatedByUser);

            modelBuilder.Entity<User>().HasOne(user => user.Role)
                .WithMany()
                .HasForeignKey(user => user.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>().Property(u => u.Password).HasMaxLength(255);
            modelBuilder.Entity<Present>().HasOne(present => present.Donor)
                .WithMany()
                .HasForeignKey(present => present.DonorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Card>().HasOne(card => card.Present)
                .WithMany()
                .HasForeignKey(card => card.PresentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Winner>().HasOne(winner => winner.Card)
                .WithMany()
                .HasForeignKey(winner => winner.CardId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Winner>().HasOne(winner => winner.Lottery)
                .WithMany()
                .HasForeignKey(winner => winner.LotteryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Winner>().HasOne(winner => winner.Present)
                .WithMany()
                .HasForeignKey(winner => winner.PresentId)
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