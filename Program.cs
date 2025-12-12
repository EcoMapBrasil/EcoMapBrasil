using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using EcoMapBrasil.Data;
using EcoMapBrasil.Models;

var builder = WebApplication.CreateBuilder(args);

// serviços existentes...
builder.Services.AddControllersWithViews();

// autenticação por cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login/AccessDenied";
    });

builder.Services.AddAuthorization();

// register the DbContext using the connection string
try
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
catch (Exception ex)
{
    Console.WriteLine("AddDbContext failed: " + ex);
    throw;
}

Console.WriteLine("=== Registered service types ===");
foreach (var sd in builder.Services)
{
    Console.WriteLine($"{sd.ServiceType?.FullName} => Impl: {sd.ImplementationType?.FullName} Lifetime:{sd.Lifetime}");
}

var app = builder.Build();

// middleware existente...
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();    
app.UseAuthorization();

// ensure DB exists and is migrated before touching it
using (var migrationScope = app.Services.CreateScope())
{
    var migrationDb = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await migrationDb.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("Migration failed: " + ex);
        throw;
    }
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!await db.Usuarios.AnyAsync(u => u.Email == "admin@local"))
    {
        db.Usuarios.Add(new Usuario { Nome = "Administrador", Email = "admin@local", Senha = "admin123", Tipo = "ADM" });
        await db.SaveChangesAsync();
    }
}

app.MapDefaultControllerRoute();
app.Run();

// Adicionando a migração inicial e atualizando o banco de dados
void EnsureDatabaseMigrated()
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Adiciona a migração inicial, se ainda não foi aplicada
    if (!db.Database.GetAppliedMigrations().Any())
    {
        db.Database.Migrate();
    }
}

EnsureDatabaseMigrated();


