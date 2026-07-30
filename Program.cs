using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── DUAL WRITE: SQL Server (local SSMS) + PostgreSQL (Render) ──

// 1. SQL Server desde appsettings.json (SIEMPRE disponible)
var sqlConnStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
if (!string.IsNullOrEmpty(sqlConnStr))
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(sqlConnStr));
}

// 2. PostgreSQL desde DATABASE_URL (solo cuando existe, ej. en Render)
var pgRawUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string? pgConnStr = null;
if (!string.IsNullOrEmpty(pgRawUrl) && (pgRawUrl.StartsWith("postgresql://") || pgRawUrl.StartsWith("postgres://")))
{
    var uri = new Uri(pgRawUrl);
    var userInfo = uri.UserInfo.Split(':');
    var port = uri.Port > 0 ? uri.Port : 5432;
    pgConnStr = $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(pgConnStr));
}

// 3. DualWriteService: usa SQL Server como primaria, PostgreSQL como secundaria
builder.Services.AddScoped<DualWriteService>(sp =>
{
    var primary = sp.GetRequiredService<AppDbContext>();
    var secondaryFactory = pgConnStr != null
        ? sp.GetRequiredService<IDbContextFactory<AppDbContext>>()
        : null;
    return new DualWriteService(primary, secondaryFactory);
});

builder.Services.AddScoped<PasswordService>();
builder.Services.AddHttpClient<EmailService>();

var app = builder.Build();

// Crear tablas en AMBAS bases de datos
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var primary = sp.GetRequiredService<AppDbContext>();
    primary.Database.EnsureCreated();

    if (pgConnStr != null)
    {
        var secondaryFactory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var secondary = secondaryFactory.CreateDbContext();
        secondary.Database.EnsureCreated();
    }
}

// TODO: quitar swagger en produccion despues de pruebas
app.UseSwagger();
app.UseSwaggerUI();

// Comentado en desarrollo para permitir llamadas HTTP desde el emulador Android (http://10.0.2.2:5172)
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
