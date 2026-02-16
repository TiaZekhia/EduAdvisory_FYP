using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Interfaces.Services;
using EduAdvisory_Backend.Services;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Repositories;

namespace EduAdvisory_Backend.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register application services
            services.AddScoped<IStudentAnalysisService, StudentAnalysisService>();

            // Register repositories
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IStudyGuideRepository, StudyGuideRepository>();
            services.AddScoped<ICoursePrerequisiteRepository, CoursePrerequisiteRepository>();

            return services;
        }

        public static IServiceCollection AddDatabaseConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<EduAdvisoryDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            );

            return services;
        }

        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            return services;
        }

        public static IServiceCollection AddKeycloakAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var keycloakSettings = configuration.GetSection("Keycloak");
            var authority = keycloakSettings["Authority"];
            var audience = keycloakSettings["Audience"];

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.RequireHttpsMetadata = false;
                    options.Audience = audience;
                    options.MetadataAddress = $"{authority}/.well-known/openid-configuration";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudiences = new[] { audience, "account" },
                        ValidateIssuer = true,
                        ValidIssuer = authority,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(5),
                        NameClaimType = "preferred_username",
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                        ValidateIssuerSigningKey = false,
                        RequireSignedTokens = false
                    };

                    options.Events = JwtBearerEventsConfiguration.Configure();
                });

            services.AddAuthorization();

            return services;
        }
    }
}