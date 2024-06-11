using DatalakeApiClasses.Exceptions.Base;
using DatalakeDatabase;
using DatalakeDatabase.Repositories;
using DatalakeServer.BackgroundServices.Collector;
using DatalakeServer.BackgroundServices.Collector.Collectors.Factory;
using DatalakeServer.Constants;
using DatalakeServer.Middlewares;
using DatalakeServer.Services.Receiver;
using DatalakeServer.Services.SessionManager;
using LinqToDB;
using LinqToDB.AspNet;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NJsonSchema.Generation;
using Serilog;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;

#if DEBUG
using LinqToDB.AspNet.Logging;
#endif

namespace DatalakeServer
{
	/// <summary>
	/// �������� ����� ����������
	/// </summary>
	public class Program
	{
		/// <summary>
		/// ����� ������� ����������
		/// </summary>
		/// <param name="args">���������, � �������� ��� �����������</param>
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

			builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));
			builder.Services.AddLogging(options => options.AddSerilog());
			builder.Services.AddEndpointsApiExplorer();

			ConfigureDatabase(builder);
			ConfigureServices(builder);

			var app = builder.Build();

			app.UseSerilogRequestLogging();

			StartWorkWithDatabase(app);

			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
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
					]);
			});
#if DEBUG
			app.UseOpenApi();
			app.UseSwaggerUi();
#endif
			app.UseMiddleware<AuthMiddleware>();

			ConfigureErrorPage(app);

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
			app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");

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
					.UseNpgsql(connectionString)
#if DEBUG
					.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug()))
#endif
					;
			});

			builder.Services.AddLinqToDBContext<DatalakeContext>((provider, options) =>
				options
					.UsePostgreSQL(connectionString ?? throw new Exception("�� �������� ������ ����������� � ���� ������"))
#if DEBUG
					.UseDefaultLogging(provider)
#endif
			);
		}

		static void ConfigureServices(WebApplicationBuilder builder)
		{
			// ����������
			builder.Services.AddSingleton<CollectorFactory>();
			builder.Services.AddSingleton<ReceiverService>();
			builder.Services.AddSingleton<SessionManagerService>();

			// ���������
			builder.Services.AddTransient<BlocksRepository>();
			builder.Services.AddTransient<TagsRepository>();
			builder.Services.AddTransient<SourcesRepository>();
			builder.Services.AddTransient<UsersRepository>();
			builder.Services.AddTransient<UserGroupsRepository>();
			builder.Services.AddTransient<ValuesRepository>();
			builder.Services.AddTransient<AuthMiddleware>();

			// ������
			builder.Services.AddHostedService<CollectorService>();
		}

		static async void StartWorkWithDatabase(WebApplication app)
		{
			using var serviceScope = app.Services?.GetService<IServiceScopeFactory>()?.CreateScope();

			var context = serviceScope?.ServiceProvider.GetRequiredService<DatalakeEfContext>();
			context?.Database.Migrate();

			DatalakeContext.SetupLinqToDB();
			var db = serviceScope?.ServiceProvider.GetRequiredService<DatalakeContext>();
			if (db != null)
				await db.EnsureDataCreatedAsync();
		}

		static void ConfigureErrorPage(WebApplication app)
		{
			app.UseExceptionHandler(exceptionHandlerApp =>
			{
				exceptionHandlerApp.Run(async context =>
				{
					var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

					var error = exceptionHandlerPathFeature?.Error;
					string message;

					if (error is DatalakeException)
					{
						message = error.ToString();
					}
					else
					{
						message = "������ ���������� �� �������" +
							"\n\n" + // �����������, �� �������� ������ �������� ��������� ����� ���������
							error?.ToString() ?? "error is null";
					}

					context.Response.StatusCode = StatusCodes.Status500InternalServerError;
					context.Response.ContentType = "text/plain; charset=UTF-8";

					await context.Response.WriteAsync(message);
				});
			});
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
