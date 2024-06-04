using DatalakeApiClasses.Exceptions.Base;
using DatalakeServer.BackgroundServices.Collector;
using DatalakeServer.BackgroundServices.Collector.Collectors.Factory;
using DatalakeServer.Constants;
using DatalakeServer.Middlewares;
using DatalakeServer.Services.Receiver;
using DatalakeServer.Services.SessionManager;
using DatalakeDatabase;
using DatalakeDatabase.Repositories;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
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

namespace DatalakeServer
{
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
				options.SchemaSettings.SchemaProcessors.Add(new XEnumVarnamesNswagSchemaProcessor());
			});

			builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));
			builder.Services.AddLogging(options => options.AddSerilog());

			ConfigureDatabase(builder);
			ConfigureAuth(builder);
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
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseCors(policy =>
			{
				policy
					.AllowAnyMethod()
					.AllowAnyOrigin()
					.AllowAnyHeader()
					.WithExposedHeaders([
						AuthConstants.AccessHeader,
						AuthConstants.TokenHeader,
						AuthConstants.NameHeader,
					]);
			});
			app.UseOpenApi();
			app.UseSwaggerUi();
			app.UseMiddleware<AuthMiddleware>();

			ConfigureErrorPage(app);

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}

		static void ConfigureDatabase(WebApplicationBuilder builder)
		{
			var connectionString = builder.Configuration.GetConnectionString("Default");

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
					.UsePostgreSQL(connectionString ?? throw new Exception("Не передана строка подключения к базе данных"))
#if DEBUG
					.UseDefaultLogging(provider)
#endif
			);
		}

		static void ConfigureAuth(WebApplicationBuilder builder)
		{
			builder.Services
				.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
				})
				.AddCookie(options =>
				{
					options.LoginPath = "/account/login";
					options.AccessDeniedPath = "/account/noAccess";
					options.LogoutPath = "/account/logout";
				})
				.AddOpenIdConnect(options =>
				{
					options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.Authority = "http://localhost:9090/realms/energo/";
					options.ClientId = "datalake";
					options.ClientSecret = "8q9UuxZplUNgusxx7dnGZl3xTP7Ce56q";
					options.MetadataAddress = "http://localhost:9090/realms/energo/.well-known/openid-configuration";
					options.RequireHttpsMetadata = false;
					options.GetClaimsFromUserInfoEndpoint = true;
					options.Scope.Add("openid");
					options.Scope.Add("profile");
					options.SaveTokens = true;
					options.ResponseType = OpenIdConnectResponseType.Code;
					options.NonceCookie.SameSite = SameSiteMode.Unspecified;
					options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
					options.TokenValidationParameters = new TokenValidationParameters
					{
						NameClaimType = "name",
						RoleClaimType = ClaimTypes.Role,
						ValidateIssuer = true
					};
				});

			builder.Services
				.AddAuthorizationBuilder()
					.AddPolicy("user", policy =>
						policy.RequireAssertion(context =>
							context.User.HasClaim(c => c.Type == "realm_access" && c.Value.Contains("datalake_user"))))
					.AddPolicy("admin", policy =>
						policy.RequireAssertion(context =>
							context.User.HasClaim(c => c.Type == "realm_access" && c.Value.Contains("datalake_admin"))));
		}

		static void ConfigureServices(WebApplicationBuilder builder)
		{
			// постоянные
			builder.Services.AddSingleton<CollectorFactory>();
			builder.Services.AddSingleton<ReceiverService>();
			builder.Services.AddSingleton<SessionManagerService>();

			// временные
			builder.Services.AddTransient<BlocksRepository>();
			builder.Services.AddTransient<TagsRepository>();
			builder.Services.AddTransient<SourcesRepository>();
			builder.Services.AddTransient<UsersRepository>();
			builder.Services.AddTransient<UserGroupsRepository>();
			builder.Services.AddTransient<ValuesRepository>();
			builder.Services.AddTransient<AuthMiddleware>();

			// службы
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
						message = "Ошибка выполнения на сервере" +
							"\n\n" + // разделитель, по которому клиент отсекает служебную часть сообщения
							error?.ToString() ?? "error is null";
					}

					context.Response.StatusCode = StatusCodes.Status500InternalServerError;
					context.Response.ContentType = "text/plain; charset=UTF-8";

					await context.Response.WriteAsync(message);
				});
			});
		}

		public class XEnumVarnamesNswagSchemaProcessor : ISchemaProcessor
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
