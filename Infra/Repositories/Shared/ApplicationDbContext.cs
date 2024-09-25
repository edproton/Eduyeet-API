using Domain.Entities;
using Infra.ValueObjects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Shared;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext(options)
{
    
    public DbSet<LearningSystem> LearningSystems { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Qualification> Qualifications { get; set; }
    
    public DbSet<Booking> Bookings { get; set; }
    
    public DbSet<Availability> Availabilities { get; set; }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Person> Persons { get; set; }

    public DbSet<Tutor> Tutors { get; set; }
    
    public DbSet<Student> Students { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LearningSystem>(entity =>
        {
            entity.HasMany(ls => ls.Subjects)
                .WithOne(s => s.LearningSystem)
                .HasForeignKey(s => s.LearningSystemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasMany(s => s.Qualifications)
                .WithOne(q => q.Subject)
                .HasForeignKey(q => q.QualificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(au => au.Person)
            .WithOne()
            .HasForeignKey<ApplicationUser>(au => au.PersonId);
    }
}