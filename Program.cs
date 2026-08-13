using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── CORS para la web Angular ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirWeb", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── DUAL WRITE: SQL Server local (SSMS) + SQL Server Somee (nube) ──

var someeConnStr = Environment.GetEnvironmentVariable("SOMEE_CONNECTION_STRING");
var localConnStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

bool useSomee = !string.IsNullOrEmpty(someeConnStr);
bool hasLocal = !string.IsNullOrEmpty(localConnStr);

if (useSomee)
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(someeConnStr));
    if (hasLocal)
        builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(localConnStr), ServiceLifetime.Scoped);
}
else if (hasLocal)
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(localConnStr));
}

builder.Services.AddScoped<DualWriteService>(sp =>
{
    var primary = sp.GetRequiredService<AppDbContext>();
    IDbContextFactory<AppDbContext>? secondaryFactory = null;
    if (useSomee && hasLocal)
        secondaryFactory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
    return new DualWriteService(primary, secondaryFactory);
});

builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddHttpClient("resend");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Asegurar tablas nuevas de la web + columna rol, y sembrar el admin
    var hashAdmin = sp.GetRequiredService<PasswordService>().Encriptar("Admin123");
    DbInitializer.AsegurarEsquema(db);
    DbInitializer.SembrarAdmin(db, hashAdmin);

    // Mantener también la BD secundaria (local SSMS) con el mismo esquema
    if (useSomee && hasLocal)
    {
        using var secondary = sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
        DbInitializer.AsegurarEsquema(secondary);
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("PermitirWeb");
app.UseAuthorization();
app.MapControllers();
app.Run();
