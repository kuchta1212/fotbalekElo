using System.Text.Json;
using Elo_fotbalek.Authentication;
using Elo_fotbalek.Configuration;
using Elo_fotbalek.EloCounter;
using Elo_fotbalek.Models;
using Elo_fotbalek.Storage;
using Elo_fotbalek.TeamGenerator;
using Elo_fotbalek.TrendCalculator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Elo_fotbalek
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            ConfigureMiddleware(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Authentication and Authorization
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Admin/Login";
                    options.Events.OnRedirectToLogin = context =>
                    {
                        // For API requests, return 401 instead of redirecting to login page
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = 401;
                            return Task.CompletedTask;
                        }
                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    };
                })
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuth", null);

            services.AddAuthorization(options =>
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, "BasicAuth");
                    policy.RequireRole("Administrator");
                })
            );

            // Configuration Options
            services.Configure<AppConfigurationOptions>(configuration.GetSection(AppConfigurationOptions.AppConfiguration));
            services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.BlobStorage));

            // Storage - choose offline or Azure based on config
            var useOffline = configuration.GetValue<bool>(BlobStorageOptions.BlobStorage + ":UseOffline");
            if (useOffline)
            {
                services.AddSingleton<IBlobClient, OfflineBlobClient>();
            }
            else
            {
                services.AddSingleton<IBlobClient, BlobClient>();
            }

            // Business Logic Services
            services.AddTransient<IModelCreator, ModelCreator>();
            services.AddTransient<IEloCalulator, Elo_fotbalek.EloCounter.EloCalculator>();
            services.AddTransient<ITeamGenerator, TeamGenerator.TeamGenerator>();
            services.AddTransient<ITrendCalculator, TrendCalculator.TrendCalculator>();

            // Add Controllers (for API and legacy admin routes)
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

            // CORS configuration from appsettings
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                    else
                    {
                        // Fallback if no origins configured
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            // Enable CORS before any other middleware
            app.UseCors("CorsPolicy");

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Map API controllers explicitly
            app.MapControllers();

            // SPA fallback - serve index.html for all non-API routes
            // This ensures React app handles routing, not MVC
            app.MapFallbackToFile("index.html");
        }
    }
}
