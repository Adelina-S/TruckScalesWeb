using Microsoft.EntityFrameworkCore;
using TruckScalesWeb.Models;

namespace TruckScalesWeb.DAO
{
    public class SqlContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<CarType> CarTypes { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<OneWeighing> OneWeighings { get; set; }
        public DbSet<Weighing> Weighings { get; set; }
        public DbSet<Photo> Photo { get; set; }
        public SqlContext(DbContextOptions<SqlContext> options)
            : base(options)
        {
            try
            {
                Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity(j => j.ToTable("UserRoles"));

            modelBuilder.Entity<Weighing>()
                .HasMany(w => w.Cars)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "WeighingCar",
                    j => j.HasOne<Car>().WithMany().OnDelete(DeleteBehavior.Restrict),
                    j => j.HasOne<Weighing>().WithMany().OnDelete(DeleteBehavior.Cascade)
                );

            modelBuilder.Entity<Weighing>()
                .HasOne(w => w.Material)
                .WithMany()
                .HasForeignKey(w => w.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Weighing>()
                .HasMany(w => w.OneWeighings) 
                .WithOne()
                .HasForeignKey(ow => ow.WeighingId) 
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Weighing>()
               .HasOne(w => w.Operator)
               .WithMany()
               .HasForeignKey(w => w.OperatorId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OneWeighing>()
                .HasOne(ow => ow.Operator) 
                .WithMany()
                .HasForeignKey("OperatorId") 
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OneWeighing>()
                .HasMany(ow => ow.Photos)
                .WithOne(p => p.OneWeighing)
                .HasForeignKey(p => p.OneWeighingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Photo>()
                .HasOne(p => p.PhotoContent)
                .WithOne(pc => pc.Photo)
                .HasForeignKey<PhotoContent>(pc => pc.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PhotoContent>()
                .Property(pc => pc.ByteArray)
                .IsRequired();

            modelBuilder.Entity<Car>().Navigation(t => t.Country).AutoInclude();
            modelBuilder.Entity<Car>().Navigation(t => t.CarType).AutoInclude();

            modelBuilder.Entity<OneWeighing>().Navigation(t => t.Operator).AutoInclude();
            modelBuilder.Entity<OneWeighing>().Navigation(t => t.Photos).AutoInclude();

            modelBuilder.Entity<Weighing>().Navigation(t=>t.OneWeighings).AutoInclude();
            modelBuilder.Entity<Weighing>().Navigation(t=>t.Cars).AutoInclude();
            modelBuilder.Entity<Weighing>().Navigation(t => t.Material).AutoInclude();
            modelBuilder.Entity<Weighing>().Navigation(t=>t.Operator).AutoInclude();
        }
    }
}
