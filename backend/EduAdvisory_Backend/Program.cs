using Microsoft.EntityFrameworkCore;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Interfaces.Services;
using EduAdvisory_Backend.Services;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<EduAdvisoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IStudentAnalysisService, StudentAnalysisService>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudyGuideRepository, StudyGuideRepository>();
builder.Services.AddScoped<ICoursePrerequisiteRepository, CoursePrerequisiteRepository>();



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
