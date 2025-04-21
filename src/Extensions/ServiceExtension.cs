// src\Extensions\ServiceExtension.cs

using backend.Security.Configuration;
using backend.Data;
using backend.Entities.Users;
using backend.Security.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace backend.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Configures the database connection using the provided settings.
        /// </summary>
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddDbContext<PrototypeDbContext>(options => 
                options.UseNpgsql(configuration.GetConnectionString("DbConnection")));

            return services;
        }

        /// <summary>
        /// Configures the Identity system with custom settings for password validation.
        /// </summary>
        public static IServiceCollection ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                // Configure password requirements
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<PrototypeDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }

        /// <summary>
        /// Configures JWT authentication with the provided settings for token validation.
        /// </summary>
        public static IServiceCollection ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettingsSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettingsSection);

            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
            var key = Encoding.UTF8.GetBytes(jwtSettings!.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true
                };
            });

            return services;
        }

        /// <summary>
        /// Configures authorization policies for custom requirements.
        /// </summary>
        public static IServiceCollection ConfigureAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationHandler, OwnDataAuthorizationHandler>();
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("OwnData", policy =>
                    policy.Requirements.Add(new OwnDataRequirement()));
            });

            return services;
        }

        /// <summary>
        /// Configures Swagger API documentation and security definitions.
        /// </summary>
        public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            return services;
        }
    

        /// <summary>
        /// Configures all necessary services including database, identity, JWT, authorization, and Swagger.
        /// </summary>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.ConfigureDatabase(configuration, env)
                    .ConfigureIdentity()
                    .ConfigureJwt(configuration)
                    .ConfigureAuthorizationPolicies()
                    .ConfigureSwagger();

            // Configuring Identity options for allowed characters in usernames
            services.Configure<IdentityOptions>(options =>
            {
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            });

            // Configure external HTTP client (example: for Hugging Face API)
            services.AddHttpClient("HF", client =>
            {
                client.BaseAddress = new Uri("http://huggingface:5000");
                client.Timeout = Timeout.InfiniteTimeSpan;
            });

            return services;
        }
    }
}