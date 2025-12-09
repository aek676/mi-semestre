using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MiCuatri.API.Data;      // Namespace correcto
using MiCuatri.API.Services;  // Namespace correcto

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN BASE DE DATOS (MongoDB) ---
// Leemos la sección "MiCuatriDatabase" de tu appsettings.json
var mongoSettings = builder.Configuration.GetSection("MiCuatriDatabase");
var connectionString = mongoSettings["ConnectionStrings"];
var databaseName = mongoSettings["DatabaseName"];

// Validación rápida
if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
{
    throw new InvalidOperationException("Falta configuración de MongoDB en appsettings.json");
}

var mongoClient = new MongoClient(connectionString);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMongoDB(mongoClient, databaseName)
);

// --- 2. SERVICIOS ---
builder.Services.AddScoped<BlackboardAuthService>();

// --- 3. API Y SWAGGER ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Usamos Swagger para la UI

var app = builder.Build();

// --- 4. PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();