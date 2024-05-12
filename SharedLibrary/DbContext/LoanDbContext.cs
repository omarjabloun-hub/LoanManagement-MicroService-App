using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SharedLibrary.DbContext;

public class LoanDbContext(IConfiguration configuration) : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<LoanApplication> LoanApplications { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(configuration["POSTGRES_CONNECTION_STRING"]);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {   
        // Neglect documents inside LoanApplication entity
        // Documents will be saved in a separate DB
        modelBuilder.Entity<LoanApplication>()
            .Ignore(x => x.Documents)
            .HasKey(x => x.Id);
        
    }
    
}