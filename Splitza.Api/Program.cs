using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Splitza.Api.Data;
using Splitza.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=splitza;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString));

// Services
builder.Services.AddScoped<SessionService>();
builder.Services.AddSingleton<BillCalculationService>();
builder.Services.AddSingleton<ReceiptParserService>();

// OCR: swap MockOcrService for a real implementation here when ready
builder.Services.AddScoped<IOcrService, MockOcrService>();

// CORS for mobile app dev
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Scalar UI: http://localhost:5000/scalar/v1
    app.MapScalarApiReference();
}

app.UseCors();
app.MapControllers();

// Apply migrations and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
