using KursovaHomeGarden.Areas.Identity.Data;
using KursovaHomeGarden.Models;
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
    public DbSet<Fertilize> Fertilizes { get; set; }
    public DbSet<ActionFrequency> ActionFrequencies { get; set; }
    public DbSet<Season> Seasons { get; set; }
    public DbSet<SunlightRequirement> SunlightRequirements { get; set; }
    public DbSet<ActionType> ActionTypes { get; set; }


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

        builder.Entity<ActionFrequency>()
           .HasOne(af => af.Plant)
           .WithMany(p => p.ActionFrequencies)
           .HasForeignKey(af => af.plant_id)
           .IsRequired();

        builder.Entity<ActionFrequency>()
            .HasOne(af => af.Season)
            .WithMany()
            .HasForeignKey(af => af.season_id)
            .IsRequired();

        builder.Entity<ActionFrequency>()
            .HasOne(af => af.ActionType)
            .WithMany()
            .HasForeignKey(af => af.action_type_id)
            .IsRequired();

  



        builder.Entity<Plant>()
        .HasOne(p => p.SunlightRequirement)
        .WithOne(sr => sr.Plant)
        .HasForeignKey<SunlightRequirement>(sr => sr.plant_id);

        builder.Entity<ActionFrequency>()
              .Property(af => af.volume)
              .HasColumnType("decimal(10,2)");

        builder.Entity<Season>()
            .Property(s => s.temperature_range_max)
            .HasColumnType("decimal(5,1)");

        builder.Entity<Season>()
            .Property(s => s.temperature_range_min)
            .HasColumnType("decimal(5,1)");
    }
}
