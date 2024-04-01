using DatalakeApp.Services;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace DatalakeDatabase
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddControllers();
			builder.Services.AddSwaggerDocument();

			ConfigureDatabase(builder);
			ConfigureAuth(builder);
			ConfigureServices(builder);

			var app = builder.Build();

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
				options.UseNpgsql(connectionString, b => b.MigrationsAssembly(nameof(DatalakeApp)));
			});
			builder.Services.AddLinqToDBContext<DatalakeContext>((provider, options) =>
				options
					.UsePostgreSQL(connectionString ?? throw new Exception("Не передана строка подключения к базе данных"))
					.UseDefaultLogging(provider));
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
							context.User.HasClaim(c => (c.Type == "realm_access") && c.Value.Contains("datalake_user"))))
					.AddPolicy("admin", policy =>
						policy.RequireAssertion(context =>
							context.User.HasClaim(c => (c.Type == "realm_access") && c.Value.Contains("datalake_admin"))));
		}

		static void ConfigureServices(WebApplicationBuilder builder)
		{
			builder.Services.AddScoped<ReceiverService>();
			builder.Services.AddScoped<HistoryService>();
		}

		static async void StartWorkWithDatabase(WebApplication app)
		{
			using var serviceScope = app.Services?.GetService<IServiceScopeFactory>()?.CreateScope();

			var context = serviceScope?.ServiceProvider.GetRequiredService<DatalakeEfContext>();
			context?.Database.Migrate();

			var db = serviceScope?.ServiceProvider.GetRequiredService<DatalakeContext>();
			if (db != null)
				await db.EnsureDataCreatedAsync();
		}
	}
}
