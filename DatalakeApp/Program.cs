using DatalakeApp.BackgroundSerivces.Collector;
using DatalakeApp.BackgroundSerivces.Collector.Collectors.Factory;
using DatalakeApp.Services.Receiver;
using DatalakeDatabase;
using DatalakeDatabase.Repositories;
using LinqToDB;
using LinqToDB.AspNet;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;

namespace DatalakeApp
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddControllers();
			builder.Services.AddSwaggerDocument(options =>
			{
				options.DocumentName = "Datalake App";
				options.Title = "Datalake App";
				options.Version = "v1";
			});

			ConfigureDatabase(builder);
			ConfigureAuth(builder);
			ConfigureServices(builder);

			builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));
			builder.Services.AddLogging(options => options.AddSerilog());

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
			app.UseCors(policy => policy.AllowAnyOrigin());
			app.UseOpenApi();
			app.UseSwaggerUi();

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
					.UseNpgsql(connectionString);
			});

			builder.Services.AddLinqToDBContext<DatalakeContext>((provider, options) =>
				options
					.UsePostgreSQL(connectionString ?? throw new Exception("Не передана строка подключения к базе данных"))
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

			// временные
			builder.Services.AddTransient<BlocksRepository>();
			builder.Services.AddTransient<TagsRepository>();
			builder.Services.AddTransient<SourcesRepository>();
			builder.Services.AddTransient<UsersRepository>();
			builder.Services.AddTransient<ValuesRepository>();

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
	}
}
