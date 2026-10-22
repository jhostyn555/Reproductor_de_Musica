using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<EcommerceApp.Services.CloudinaryService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Crear roles por defecto y aplicar migraciones de forma segura
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed Admin por defecto
        const string adminEmail    = "admin@ecommerce.com";
        const string adminPassword = "Admin123!";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email    = adminEmail,
                FullName = "Administrador",
                EmailConfirmed = true
            };
            var res = await userManager.CreateAsync(admin, adminPassword);
            if (res.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed Usuario normal por defecto
        const string userEmail    = "user@ecommerce.com";
        const string userPassword = "User123!";
        if (await userManager.FindByEmailAsync(userEmail) is null)
        {
            var user = new ApplicationUser
            {
                UserName = userEmail,
                Email    = userEmail,
                FullName = "Usuario Demo",
                EmailConfirmed = true
            };
            var res = await userManager.CreateAsync(user, userPassword);
            if (res.Succeeded)
                await userManager.AddToRoleAsync(user, "User");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones o inicializar los roles.");
    }
}

app.Run();