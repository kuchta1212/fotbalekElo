﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Configuration;
using Elo_fotbalek.EloCounter;
using Elo_fotbalek.Models;
using Elo_fotbalek.Storage;
using Elo_fotbalek.TeamGenerator;
using Elo_fotbalek.TrendCalculator;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Elo_fotbalek
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Admin/Login";
                });

            services.AddAuthorization(options =>
                options.AddPolicy("MyPolicy",
                    policy =>
                    {
                        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
                        policy.RequireRole("Administrator");
                    })
                );

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<AppConfigurationOptions>(Configuration.GetSection(AppConfigurationOptions.AppConfiguration));
            services.Configure<BlobStorageOptions>(Configuration.GetSection(BlobStorageOptions.BlobStorage));

            var useOffline = Configuration.GetValue<bool>(BlobStorageOptions.BlobStorage+":UseOffline");
            if (useOffline)
            {
                services.AddSingleton<IBlobClient, OfflineBlobClient>();
            }
            else
            {
                services.AddSingleton<IBlobClient, BlobClient>();
            }

            services.AddTransient<IModelCreator, ModelCreator>();
            services.AddTransient<IEloCalulator, EloCounter.EloCalculator>();
            services.AddTransient<ITeamGenerator, TeamGenerator.TeamGenerator>();
            services.AddTransient<ITrendCalculator, TrendCalculator.TrendCalculator>();

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
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
            app.UseCookiePolicy();

            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
