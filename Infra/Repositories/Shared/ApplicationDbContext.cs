using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Shared;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    
    public DbSet<LearningSystem> LearningSystems { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Qualification> Qualifications { get; set; }

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
                .HasForeignKey(q => q.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}