using CriminalNetwork.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CriminalNetwork.Api.Data;

// Connects the C# models to the SQLite tables.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Suspect> Suspects => Set<Suspect>();
    public DbSet<Crime> Crimes => Set<Crime>();
    public DbSet<Relationship> Relationships => Set<Relationship>();
    public DbSet<CrimeSuspect> CrimeSuspects => Set<CrimeSuspect>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Suspect>().ToTable("Suspects");
        modelBuilder.Entity<Crime>().ToTable("Crimes");
        modelBuilder.Entity<Relationship>().ToTable("Relationships");
        modelBuilder.Entity<CrimeSuspect>().ToTable("CrimeSuspects");

        modelBuilder.Entity<Relationship>()
            .HasOne(relationship => relationship.FirstSuspect)
            .WithMany()
            .HasForeignKey(relationship => relationship.FirstSuspectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Relationship>()
            .HasOne(relationship => relationship.SecondSuspect)
            .WithMany()
            .HasForeignKey(relationship => relationship.SecondSuspectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CrimeSuspect>()
            .HasOne(link => link.Crime)
            .WithMany()
            .HasForeignKey(link => link.CrimeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CrimeSuspect>()
            .HasOne(link => link.Suspect)
            .WithMany()
            .HasForeignKey(link => link.SuspectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CrimeSuspect>()
            .HasIndex(link => new { link.CrimeId, link.SuspectId })
            .IsUnique();
    }
}
