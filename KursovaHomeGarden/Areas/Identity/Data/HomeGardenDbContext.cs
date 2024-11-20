using KursovaHomeGarden.Areas.Identity.Data;
using KursovaHomeGarden.Models.CareLevel;
using KursovaHomeGarden.Models.Category;
using KursovaHomeGarden.Models.Plant;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace KursovaHomeGarden.Data;

public class HomeGardenDbContext : IdentityDbContext<ApplicationUser>
{
    public HomeGardenDbContext(DbContextOptions<HomeGardenDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Plant> Plants { get; set; }
    public DbSet<CareLevel> CareLevels { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        // Configure Plant-Category relationship
        builder.Entity<Plant>()
            .HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.category_id);

        // Configure Plant-CareLevel relationship
        builder.Entity<Plant>()
            .HasOne(p => p.CareLevel)
            .WithMany()
            .HasForeignKey(p => p.care_level_id);
    }
}
