using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApp
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
            services.AddDbContext<AppDbContext>(options => 
                options.UseSqlite(Configuration.GetConnectionString("SqliteConnection")));
            
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            UpdateDatabase(app, Configuration);
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapRazorPages(); });
        }
        
        private static void UpdateDatabase(IApplicationBuilder app, IConfiguration configuration)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            using var ctx = serviceScope.ServiceProvider.GetService<AppDbContext>();

            if (configuration["AppDataInitialization:DropDatabase"] == "True")
            {
                Console.WriteLine("DropDatabase");
                DAL.Helpers.DataInitializers.DeleteDatabase(ctx);
            }

            if (configuration["AppDataInitialization:MigrateDatabase"] == "True")
            {
                Console.WriteLine("MigrateDatabase");
                DAL.Helpers.DataInitializers.MigrateDatabase(ctx);
            }
            if (configuration.GetValue<bool>("AppDataInitialization:SeedData") && ctx.GameOptions.ToList().Count == 0)
            {
                Console.WriteLine("SeedData");
                DAL.Helpers.DataInitializers.SeedDefaultData(ctx);
            }
        }
    }
}