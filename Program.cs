using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── DUAL WRITE: SQL Server (local SSMS) + PostgreSQL (Render) ──

var sqlConnStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var pgRawUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

string? pgConnStr = null;
if (!string.IsNullOrEmpty(pgRawUrl) && (pgRawUrl.StartsWith("postgresql://") || pgRawUrl.StartsWith("postgres://")))
{
    var uri = new Uri(pgRawUrl);
    var userInfo = uri.UserInfo.Split(':');
    var port = uri.Port > 0 ? uri.Port : 5432;
    pgConnStr = $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}

// Si estamos en Render (DATABASE_URL existe), PostgreSQL es la primaria
// Si estamos local, SQL Server es la primaria y PostgreSQL es secundaria (si hay DATABASE_URL)
bool inRender = pgConnStr != null;

if (inRender)
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(pgConnStr));
}
else
{
    if (!string.IsNullOrEmpty(sqlConnStr))
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(sqlConnStr));
}

// Para dual write local: PostgreSQL como secundaria vía factory
if (inRender == false && pgConnStr != null)
{
    builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(pgConnStr));
}

// DualWriteService
var hasSecondary = pgConnStr != null && !inRender;
builder.Services.AddScoped<DualWriteService>(sp =>
{
    var primary = sp.GetRequiredService<AppDbContext>();
    IDbContextFactory<AppDbContext>? secondaryFactory = null;
    if (hasSecondary)
        secondaryFactory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
    return new DualWriteService(primary, secondaryFactory);
});

builder.Services.AddScoped<PasswordService>();
builder.Services.AddHttpClient<EmailService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.Run();

// TODO: quitar swagger en produccion despues de pruebas
app.UseSwagger();
app.UseSwaggerUI();

// Comentado en desarrollo para permitir llamadas HTTP desde el emulador Android (http://10.0.2.2:5172)
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
