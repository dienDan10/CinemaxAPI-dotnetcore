using CinemaxAPI.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Data
{
    public class CinemaxServerDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Province> Provinces { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Theater> Theaters { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<ShowTime> ShowTimes { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Screen> Screens { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }
        public DbSet<Concession> Concessions { get; set; }
        public DbSet<ConcessionOrder> ConcessionOrders { get; set; }
        public DbSet<ConcessionOrderDetail> ConcessionOrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }


        public CinemaxServerDbContext(DbContextOptions<CinemaxServerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // BookingDetail relationships
            modelBuilder.Entity<BookingDetail>()
                .HasOne(bd => bd.Seat)
                .WithMany()
                .HasForeignKey(bd => bd.SeatId)
                .OnDelete(DeleteBehavior.NoAction);

            // Keep the cascade delete from Booking to BookingDetail
            modelBuilder.Entity<BookingDetail>()
                .HasOne(bd => bd.Booking)
                .WithMany(b => b.BookingDetails)
                .HasForeignKey(bd => bd.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConcessionOrder>()
                .HasMany(co => co.ConcessionOrderDetails)
                .WithOne(cod => cod.ConcessionOrder)
                .HasForeignKey(cod => cod.ConcessionOrderId)
                .OnDelete(DeleteBehavior.Cascade); // Optional

            modelBuilder.Entity<Province>().HasData(
           new Province { Id = 1, Name = "Hà Nội" },
           new Province { Id = 2, Name = "Hồ Chí Minh" });
        }

    }

}
