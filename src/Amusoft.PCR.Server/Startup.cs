using System;
using System.Collections.Generic;
using System.Linq;
using Amusoft.PCR.Model;
using Amusoft.PCR.Model.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Amusoft.PCR.Server.Areas.Identity;
using Amusoft.PCR.Server.Authorization;
using Amusoft.PCR.Server.BackgroundServices;
using Amusoft.PCR.Server.Data;
using Amusoft.PCR.Server.Dependencies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server
{
	class DelegatedContentTypeProvider : IContentTypeProvider
	{
		private IContentTypeProvider _baseImplementation = new FileExtensionContentTypeProvider();

		public readonly Dictionary<string, string> AdditionalDeclarations = new();

		public bool TryGetContentType(string subpath, out string contentType)
		{
			if (_baseImplementation.TryGetContentType(subpath, out contentType))
				return true;

			if (AdditionalDeclarations.TryGetValue(subpath, out contentType))
				return true;

			return false;
		}
	}
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IAuthorizationHandler, RoleOrAdminAuthorizationHandler>();
			services.AddDatabaseDeveloperPageExceptionFilter();
			services.AddSingleton<ApplicationStateTransmitter>();
			services.AddOptions();
			services.AddAuthorization(options =>
			{
				var builder = new AuthorizationPolicyBuilder();
				builder.RequireAuthenticatedUser();
				
				options.FallbackPolicy = builder.Build();
			});
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(
					Configuration.GetConnectionString("DefaultConnection")));

			services.PostConfigure<StaticFileOptions>(options =>
			{
				var contentTypeProvider = new DelegatedContentTypeProvider();
				contentTypeProvider.AdditionalDeclarations.Add(".apk", "application/vnd.android.package-archive");
				options.ContentTypeProvider = contentTypeProvider;
			});

			services.AddDefaultIdentity<ApplicationUser>(options =>
				{
					options.Password.RequireDigit = false;
					options.Password.RequireLowercase = false;
					options.Password.RequireNonAlphanumeric = false;
					options.Password.RequireUppercase = false;
					options.Password.RequiredLength = 3;
					options.Password.RequiredUniqueChars = 1;

					options.SignIn.RequireConfirmedAccount = true;
				})
				.AddUserManager<UserManager<ApplicationUser>>()
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>();
			
			services.Configure<DesktopIntegrationSettings>(Configuration.GetSection("ApplicationSettings:DesktopIntegration"));
			services.Configure<LanAddressBroadcastSettings>(Configuration.GetSection("ApplicationSettings:ServerUrlTransmitter"));
			services.Configure<AuthorizationSettings>(Configuration.GetSection("ApplicationSettings:AuthorizationSettings"));

			services.AddHttpContextAccessor();
			services.AddHttpClient("local", (provider, client) =>
			{
				var accessor = provider.GetRequiredService<IHttpContextAccessor>();
				client.BaseAddress = new Uri($"https://localhost:{accessor.HttpContext.Connection.LocalPort}");
			});
			services.AddControllers();
			services.AddRazorPages();
			services.AddServerSideBlazor();
			services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

			ServiceRegistrar.Register(services);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceScopeFactory serviceScopeFactory, ILogger<Startup> logger, ApplicationStateTransmitter applicationStateTransmitter)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			var contentTypeProvider = new FileExtensionContentTypeProvider();
			contentTypeProvider.Mappings[".apk"] = "application/vnd.android.package-archive";
			app.UseStaticFiles(new StaticFileOptions(){ ContentTypeProvider = contentTypeProvider, HttpsCompression = HttpsCompressionMode.Compress});
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
				endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
			});
			
			using (var serviceScope = serviceScopeFactory.CreateScope())
			{
				using (var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
				{
					var configuration = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
					var connectionBuilder =
						new SqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"));

					if (Configuration.GetValue<bool>("ApplicationSettings:DropDatabaseOnStart"))
					{
						logger.LogWarning("Dropping database because of settings value {Value}", "ApplicationSettings:DropDatabaseOnStart");
						context.Database.EnsureDeleted();
					}

					logger.LogInformation("Launching database as environment {Environment} with data source {@DataSource} and database {@Database}", env.EnvironmentName, connectionBuilder.DataSource, connectionBuilder.InitialCatalog);
					if (!context.Database.CanConnect())
					{
						logger.LogInformation("Database does not exist yet - Creating database through migrations");

						context.Database.Migrate();
					}
					else
					{
						if (context.Database.GetPendingMigrations().Any())
						{
							logger.LogInformation("Applying database migration");

							context.Database.Migrate();

							logger.LogInformation("Applying database migration done");
						}
						else
						{
							logger.LogDebug("No pending migrations found");
						}
					}
				}
			}

			applicationStateTransmitter.NotifyConfigurationDone();

			logger.LogInformation("Configuration done");
		}
	}
}
