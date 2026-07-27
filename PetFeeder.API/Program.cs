using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Conexion a BD: Render pone DATABASE_URL como variable de entorno
// En local usa SQL Server (appsettings.json), en Render usa PostgreSQL
var connStr = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (connStr.Contains("Host="))
        options.UseNpgsql(connStr);   // Render (PostgreSQL)
    else
        options.UseSqlServer(connStr); // Local (SQL Server)
});

// Servicio para encriptar/verificar contraseñas con BCrypt
builder.Services.AddScoped<PasswordService>();

builder.Services.AddScoped<EmailService>();

var app = builder.Build();

// Crear las tablas automáticamente si no existen (PostgreSQL en Render)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Comentado en desarrollo para permitir llamadas HTTP desde el emulador Android (http://10.0.2.2:5172)
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
