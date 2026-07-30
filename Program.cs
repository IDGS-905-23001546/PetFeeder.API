using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
