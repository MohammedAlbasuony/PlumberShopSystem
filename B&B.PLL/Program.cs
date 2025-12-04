using AutoMapper;
using B_B.BLL.Mapping;
using B_B.BLL.Service.Abstraction;
using B_B.BLL.Service.Implementation;
using B_B.DAL.DB;
using B_B.DAL.Repo.Abstraction;
using B_B.DAL.Repo.Implementation;
using Microsoft.EntityFrameworkCore;

namespace B_B.PLL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
   

            builder.Services.AddDbContext<ApplicationDBcontext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,              // how many times to retry
                        maxRetryDelay: TimeSpan.FromSeconds(10), // wait time between retries
                        errorNumbersToAdd: null        // you can specify error codes or leave null
                    )
                )
            );


            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAutoMapper(x => x.AddProfile(new MyProfile()));


            // Repositories and Services
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddScoped<IStockService, StockService>();
            builder.Services.AddScoped<SupplierService>();
            builder.Services.AddScoped<ClientService>();
            builder.Services.AddScoped<IReceiptService, ReceiptService>();
            builder.Services.AddScoped<IReportService, ReportService>();





        





            builder.Services.AddSession();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

         

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDBcontext>();
                await DbInitializer.SeedAsync(db);
            }


            app.Run();
        }
    }
}
