using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeathDiceServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using DeathDiceServer.Hubs;

namespace DeathDiceServer
{
    public class Startup
    {
        const string ROOTPATH = "#ROOTPATH#";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddCors(options => options.AddPolicy("CorsPolicyFor4200", builder => builder
                   .WithOrigins("http://localhost:4200")
                   .AllowCredentials()
                   .AllowAnyHeader()
                   .AllowAnyMethod())
              );
            //Data Base
            string connection = Configuration.GetConnectionString("DefaultConnection");

            if (connection.Contains(ROOTPATH))
                connection = connection.Replace(ROOTPATH, System.IO.Directory.GetCurrentDirectory());

            services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(connection));
            ////
            services.AddSignalR(hubOption=> {
                hubOption.ClientTimeoutInterval = TimeSpan.FromSeconds(10.0);
            });
            services.AddMvc();
            //// DJ 
            services.AddTransient<GameProcessHub>();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseRouting();
            app.UseCors("CorsPolicyFor4200");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<GameSearchHub>("/GameSearch");
                endpoints.MapHub<GameProcessHub>("/GameProcess");
            });
        }
    }
}
