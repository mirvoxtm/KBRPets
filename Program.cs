using System.Configuration;
using KBRPETS.Data;
using KBRPETS.Models;
using KBRPETS.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;

namespace KBRPETS {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // DBContext
            builder.Services.AddDbContext<KBRPETSContext>();
            builder.Services.AddScoped<SeedingService>();
            builder.Services.AddScoped<PetsService>();
            builder.Services.AddScoped<SolicitationsService>();
            builder.Services.AddScoped<UsersService>();

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession();

            var app = builder.Build();


            
            if (app.Environment.IsDevelopment()) {
                using (var scope = app.Services.CreateScope()) {
                    var services = scope.ServiceProvider;
                    var seedingService = services.GetRequiredService<SeedingService>();
                    seedingService.Seed();
                }
            }

            else {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=pets}/{action=index}/{id?}");

            
            app.MapControllerRoute(
                name: "redirectPainel",
                pattern: "{controller=painel}/{action=index}/{id?}");

            app.MapControllerRoute(
                name: "redirectIntegra",
                pattern: "pets/integra/{id}/{name}");

            app.MapControllerRoute(
                name: "redirectQueroAdotarPagination",
                pattern: "pets/integra/{page}");
            
            app.Run();
        }
    }
}