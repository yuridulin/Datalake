using Datalake.Database;
using Datalake.Database.Enums;
using Datalake.Server.Constants;
using Datalake.Server.Middlewares;
using Datalake.Server.Services;
using LinqToDB;
using LinqToDB.AspNet;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Generation;
using System.Reflection;

[assembly: AssemblyVersion("2.2.*")]

namespace Datalake.Server
{
	/// <summary>
	/// Основной класс приложения
	/// </summary>
	public class Program
	{
		internal static string WebRootPath { get; set; } = string.Empty;

		/// <summary>
		/// Метод запуска приложения
		/// </summary>
		/// <param name="args">Аргументы, с которыми оно запускается</param>
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
				options.SchemaSettings.SchemaProcessors.Add(new XEnumVarnamesNswagSchemaProcessor());
			});
			builder.Services.AddEndpointsApiExplorer();

			builder.AddMiddlewares();
			ConfigureDatabase(builder);
			builder.AddServices();

			var app = builder.Build();

			WebRootPath = app.Environment.WebRootPath;
			StartWorkWithDatabase(app);

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
			app.UseMiddlewares();

			app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}

		static void ConfigureDatabase(WebApplicationBuilder builder)
		{
			var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";

			var env = Environment.GetEnvironmentVariables();
			foreach (var key in env.Keys)
			{
				var arg = "${" + key + "}";
				if (connectionString.Contains(arg))
				{
					connectionString = connectionString.Replace(arg, env[key]?.ToString() ?? arg);
				}
			}

			builder.Services.AddDbContext<DatalakeEfContext>(options =>
			{
				options
					.UseNpgsql(connectionString, config => config.CommandTimeout(300));
			});

			builder.Services.AddLinqToDBContext<DatalakeContext>((provider, options) =>
				options
					.UsePostgreSQL(connectionString ?? throw new Exception("Connection string not provided"))
			);
		}

		static async void StartWorkWithDatabase(WebApplication app)
		{
			using var serviceScope = app.Services?.GetService<IServiceScopeFactory>()?.CreateScope();

			var context = serviceScope?.ServiceProvider.GetRequiredService<DatalakeEfContext>();
			context?.Database.Migrate();

			DatalakeContext.SetupLinqToDB();
			var db = serviceScope?.ServiceProvider.GetRequiredService<DatalakeContext>();
			if (db != null)
			{
				await db.EnsureDataCreatedAsync();
				await db.SystemRepository.WriteLog(
					"Сервер запущен",
					category: LogCategory.Core,
					type: LogType.Success
				);
			}
		}

		internal class XEnumVarnamesNswagSchemaProcessor : ISchemaProcessor
		{
			public void Process(SchemaProcessorContext context)
			{
				if (context.ContextualType.OriginalType.IsEnum)
				{
					if (context.Schema.ExtensionData is not null)
					{
						context.Schema.ExtensionData.Add("x-enum-varnames", context.Schema.EnumerationNames.ToArray());
					}
					else
					{
						context.Schema.ExtensionData = new Dictionary<string, object?>()
						{
								{"x-enum-varnames", context.Schema.EnumerationNames.ToArray()}
						};
					}
				}
			}
		}
	}
}
