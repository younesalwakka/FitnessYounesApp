using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FitnessYounesApp.Models;

namespace FitnessYounesApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet'ler: veritabanindaki tablolar
        public DbSet<SporSalonu> SporSalonlari { get; set; } = null!;
        public DbSet<Hizmet> Hizmetler { get; set; } = null!;
        public DbSet<Antrenor> Antrenorler { get; set; } = null!;
        public DbSet<AntrenorMusaitlik> AntrenorMusaitlikler { get; set; } = null!;
        public DbSet<UyeProfil> UyeProfiller { get; set; } = null!;
        public DbSet<Randevu> Randevular { get; set; } = null!;
        public DbSet<AntrenorHizmet> AntrenorHizmetleri { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // AntrenorHizmet icin composite key (cift anahtar)
            builder.Entity<AntrenorHizmet>()
                .HasKey(ah => new { ah.AntrenorId, ah.HizmetId });

            // AntrenorHizmet -> Antrenor iliskisi
            builder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Antrenor)
                .WithMany(a => a.AntrenorHizmetleri)
                .HasForeignKey(ah => ah.AntrenorId);

            // AntrenorHizmet -> Hizmet iliskisi
            builder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Hizmet)
                .WithMany(h => h.AntrenorHizmetleri)
                .HasForeignKey(ah => ah.HizmetId);

            // Decimal precision for Ucret fields
            builder.Entity<Hizmet>()
                .Property(h => h.Ucret)
                .HasPrecision(18, 2);

            builder.Entity<Randevu>()
                .Property(r => r.Ucret)
                .HasPrecision(18, 2);
        }
    }
}

