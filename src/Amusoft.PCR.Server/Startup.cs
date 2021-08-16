using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Model;
using Amusoft.PCR.Model.Entities;
using Amusoft.PCR.Model.Statics;
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
using Amusoft.PCR.Server.Dependencies;
using Amusoft.PCR.Server.Domain.Authorization;
using Amusoft.PCR.Server.Domain.IPC;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Amusoft.PCR.Server
{
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
			services.Configure<JwtSettings>(Configuration.GetSection("ApplicationSettings:Jwt"));
			services.Configure<DesktopIntegrationSettings>(Configuration.GetSection("ApplicationSettings:DesktopIntegration"));
			services.Configure<ClientDiscoverySettings>(Configuration.GetSection("ApplicationSettings:ServerUrlTransmitter"));
			services.Configure<StaticFileOptions>(options =>
			{
				var contentTypeProvider = new FileExtensionContentTypeProvider();
				contentTypeProvider.Mappings[".apk"] = "application/vnd.android.package-archive";
				options.ContentTypeProvider = contentTypeProvider;
			});

			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
			});
			services.AddGrpc();
			services.AddDatabaseDeveloperPageExceptionFilter();
			services.AddOptions();
			services.AddAuthorization(options =>
			{
				options.AddPolicy(PolicyNames.ApplicationPermissionPolicy, policyBuilder =>
				{
					policyBuilder.AddAuthenticationSchemes(
							CookieAuthenticationDefaults.AuthenticationScheme,
							JwtBearerDefaults.AuthenticationScheme)
						.RequireAuthenticatedUser()
						.AddRequirements(new HostCommandPermissionRequirement())
						.Build();
				});
				options.AddPolicy(PolicyNames.ApiPolicy, policyBuilder =>
				{
					policyBuilder.AddAuthenticationSchemes(
							CookieAuthenticationDefaults.AuthenticationScheme,
							JwtBearerDefaults.AuthenticationScheme)
						.RequireAuthenticatedUser()
						.Build();
				});
			});
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(
					Configuration.GetConnectionString("DefaultConnection")));

			var tokenValidationParameters = new TokenValidationParameters();
			tokenValidationParameters.ValidateIssuer = true;
			tokenValidationParameters.ValidateAudience = true;
			tokenValidationParameters.ValidateLifetime = true;
			tokenValidationParameters.ValidateIssuerSigningKey = true;
			tokenValidationParameters.ValidIssuer = Configuration["ApplicationSettings:Jwt:Issuer"];
			tokenValidationParameters.ValidAudience = Configuration["ApplicationSettings:Jwt:Issuer"];
			tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["ApplicationSettings:Jwt:Key"]));
			services.AddSingleton(tokenValidationParameters);

			services
				.AddAuthentication();

			services				  
				.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
				{				   
					options.SaveToken = true;
					options.RefreshInterval = TimeSpan.Parse(Configuration["ApplicationSettings:Jwt:RefreshAccessTokenInterval"]);
					options.TokenValidationParameters = tokenValidationParameters;

					options.Events = new JwtBearerEvents
					{
						OnAuthenticationFailed = context =>
						{
							if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
							{
								context.Response.Headers.Add("X-Token-Expired", "true");
							}

							return Task.CompletedTask;
						}
					};
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
				.AddDefaultTokenProviders()
				.AddUserManager<UserManager<ApplicationUser>>()
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>();
			
			services.AddHttpContextAccessor();
			services.AddHttpClient("local", (provider, client) =>
			{
				var accessor = provider.GetRequiredService<IHttpContextAccessor>();
				client.BaseAddress = new Uri($"https://localhost:{accessor.HttpContext.Connection.LocalPort}");
			});
			services.AddControllers();
			services.AddRazorPages();
			services.AddServerSideBlazor();

			ServiceRegistrar.Register(services);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, 
			IWebHostEnvironment env, 
			IServiceScopeFactory serviceScopeFactory, 
			ILogger<Startup> logger,
			ApplicationStateTransmitter applicationStateTransmitter)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseMigrationsEndPoint();

				app.UseSwagger();
				app.UseSwaggerUI(options =>
				{
					options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
				});
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseGrpcWeb();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGrpcService<BackendIntegrationService>().EnableGrpcWeb();
				endpoints.MapControllers();
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
				endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
			});
			

			using (var serviceScope = serviceScopeFactory.CreateScope())
			{
				var jwtSettings = serviceScope.ServiceProvider.GetRequiredService<IOptions<JwtSettings>>();

				logger.LogDebug("Authentication settings: SignIn valid: [{AccessTokenValid}] RefreshToken valid: [{RefreshTokenValid}] Refresh every: [{RefreshInterval}]", 
					jwtSettings.Value.AccessTokenValidDuration,
					jwtSettings.Value.RefreshTokenValidDuration,
					jwtSettings.Value.RefreshAccessTokenInterval);

				VerifyAuthenticationSettings(logger, jwtSettings.Value);

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

		private void VerifyAuthenticationSettings(ILogger<Startup> logger, JwtSettings jwtSettings)
		{
			if(jwtSettings.RefreshTokenValidDuration < jwtSettings.AccessTokenValidDuration)
				logger.LogError("Access tokens have to be valid for a shorter duration than refresh tokens");
			if(jwtSettings.AccessTokenValidDuration.Add(jwtSettings.RefreshAccessTokenInterval) > jwtSettings.RefreshTokenValidDuration)
				logger.LogError("AccessTokenValidDuration + RefreshAccessTokenInterval must be smaller than RefreshTokenValidDuration");
		}
	}
}
