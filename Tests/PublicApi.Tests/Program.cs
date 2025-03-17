using Datalake.PublicApi.Constants;
using System.Reflection;

namespace PublicApi.Tests;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddControllers();
		builder.Services.AddOpenApiDocument((options, services) =>
		{
			options.DocumentName = "Datalake App";
			options.Title = "Datalake App";
			options.Version = "v" + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
			options.SchemaSettings.GenerateEnumMappingDescription = true;
			options.SchemaSettings.UseXmlDocumentation = true;
		});
		builder.Services.AddEndpointsApiExplorer();

		builder.Logging.ClearProviders();
		builder.Logging.AddConsole();
		builder.Logging.AddDebug();

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Home/Error");
		}

		if (app.Environment.IsDevelopment())
		{
			app.UseOpenApi();
			app.UseSwaggerUi();
		}

		app.UseDefaultFiles();
		app.UseStaticFiles();
		app.UseRouting();
		app.UseCors(policy =>
		{
			policy
				.AllowAnyMethod()
				.AllowAnyOrigin()
				.AllowAnyHeader()
				.WithExposedHeaders([
					AuthConstants.TokenHeader,
						AuthConstants.GlobalAccessHeader,
						AuthConstants.NameHeader,
						AuthConstants.UnderlyingUserGuidHeader,
				]);
		});
		app.UseAuthorization();

		app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");
		app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

		app.Run();
	}
}
