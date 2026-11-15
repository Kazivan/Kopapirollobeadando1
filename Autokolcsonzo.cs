using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
// Legyszives ne toroljetek ki meg egyszer egesz projecktett koszi :)
namespace Autokolcsonzo
{

    public class Auto
    {
        public Auto()
        {
            Kolcsonzesek = new List<Kolcsonzes>();
        }
        public int Id { get; set; }
        public string Rendszam { get; set; } = "";
        public string MarkaTipus { get; set; } = "";
        public string Kategoria { get; set; } = "";
        public int NapiDij { get; set; }
        public bool Elerheto { get; set; }

        public virtual List<Kolcsonzes> Kolcsonzesek { get; set; }

        public override string ToString() => $"{Rendszam} - {MarkaTipus} ({NapiDij} Ft/nap)";
    }

    // Tedd ezt az osztályt az Adatmodell.cs-be a többi osztály mellé:
    public class Dolgozo
    {
        public int Id { get; set; }
        public string Nev { get; set; } = "";
        public string Jelszo { get; set; } = "";
        public string Beosztas { get; set; } = ""; // "admin", "ugyintezo", "szerelo"
        


    }



    public class Ugyfel
    {
        public Ugyfel()
        {
            Kolcsonzesek = new List<Kolcsonzes>();
        }
        public int Id { get; set; }
        public string Nev { get; set; } = "";
        public string JogosimvanySzam { get; set; } = "";
        public string Telefon { get; set; } = "";
        public virtual List<Kolcsonzes> Kolcsonzesek { get; set; }

        public override string ToString() => $"{Nev} ({JogosimvanySzam})";
    }

    public class Kolcsonzes
    {
        public int Id { get; set; }
        public int AutoId { get; set; }
        public int UgyfelId { get; set; }
        public DateTime Kezdete { get; set; }
        public DateTime? Vege { get; set; }
        public int FizetendoOsszeg { get; set; }
        public string Allapot { get; set; } = "Aktív"; // "Aktív" vagy "Lezárt"

        public virtual Auto Auto { get; set; } = null!;
        public virtual Ugyfel Ugyfel { get; set; } = null!;
    }

    public class cnAutokolcsonzo : DbContext
    {
        public virtual DbSet<Auto> Autok { get; set; }
        public virtual DbSet<Ugyfel> Ugyfelek { get; set; }
        public virtual DbSet<Kolcsonzes> Kolcsonzesek { get; set; }
        public virtual DbSet<Dolgozo> Dolgozok { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(GetConnectionString("csAutokolcsonzo"));
            }
        }

        public static string GetConnectionString(string connectionStringName)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            var configuration = configurationBuilder.Build();
            return configuration.GetConnectionString(connectionStringName)!;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Auto>().ToTable("Autok").HasKey(x => x.Id);
            modelBuilder.Entity<Ugyfel>().ToTable("Ugyfelek").HasKey(x => x.Id);
            modelBuilder.Entity<Kolcsonzes>().ToTable("Kolcsonzesek").HasKey(x => x.Id);
            modelBuilder.Entity<Dolgozo>().ToTable("Dolgozok").HasKey(x => x.Id);

            modelBuilder.Entity<Kolcsonzes>()
                .HasOne(k => k.Auto)
                .WithMany(a => a.Kolcsonzesek)
                .HasForeignKey(k => k.AutoId);

            modelBuilder.Entity<Kolcsonzes>()
                .HasOne(k => k.Ugyfel)
                .WithMany(u => u.Kolcsonzesek)
                .HasForeignKey(k => k.UgyfelId);
        }
    }
}