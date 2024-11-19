using KursovaHomeGarden.Areas.Identity.Data;
using KursovaHomeGarden.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KursovaHomeGarden.Data;

public class HomeGardenDbContext : IdentityDbContext<ApplicationUser>
{
    public HomeGardenDbContext(DbContextOptions<HomeGardenDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        base.OnModelCreating(builder);
    }
}
