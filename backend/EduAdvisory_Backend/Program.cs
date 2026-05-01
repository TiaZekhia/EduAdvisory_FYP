using EduAdvisory_Backend.Extensions;
using EduAdvisory_Backend.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddCorsConfiguration();
builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware pipeline
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseRequestLogging(); // Only log requests in development
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();