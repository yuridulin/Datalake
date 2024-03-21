using DatalakeApp.Services;
using DatalakeDb;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Datalake
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddControllersWithViews();
			builder.Services.AddDbContext<DatalakeContext>(options =>
			{
				var connectionString = builder.Configuration.GetConnectionString("Default");

				options.UseNpgsql(connectionString);
				//options.UseLinqToDB();

				LinqToDBForEFTools.Initialize(); // подключение linq2db
			});
			builder.Services.AddSingleton(new CacheService());

			var app = builder.Build();

			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthorization();
			app.UseCors(policy => policy.AllowAnyOrigin());

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}
	}
}
