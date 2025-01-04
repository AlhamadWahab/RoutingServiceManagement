
using DomainLayer.EntityModels;
using DomainLayer.Interfaces;
using IdentityLayer;
using IdentityLayer.IdnetityModels;
using IdentityLayer.Seeding;
using InfrastructureLayer.Data;
using InfrastructureLayer.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Routing_Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Routing Service Database registration:
            string connectionStringRoutingService = builder.Configuration.GetConnectionString("RoutingDbConnection") ?? "";
            builder.Services.AddDbContext<RoutingServiceDb>(op => op.UseNpgsql(connectionStringRoutingService));
            #endregion

            #region Routing Service Authenticate Services registration:
            builder.Services.AddScoped(typeof(IService<>), typeof(MainService<>));
            builder.Services.AddScoped<ICsvService, CsvService>();
            builder.Services.AddScoped<NodeEdge>();
            builder.Services.AddTransient<IAuthenticationRService, AuthenticationRService>();
            #endregion

            #region Routing Service Repositories (Unit Of Work) registration:
            builder.Services.AddScoped(typeof(IRepository), typeof(MainRepository));
            #endregion

            #region Identity and JWT Authentication:
            builder.Services.AddIdentity<RoutingServiceAppUser, IdentityRole>(option =>
            {
                option.Password.RequiredLength = 8;
                option.Password.RequireNonAlphanumeric = true;
                option.Password.RequireUppercase = true;
                option.Password.RequireLowercase = true;
                option.Password.RequireDigit = true;
            })
                .AddEntityFrameworkStores<RoutingServiceDb>()
                .AddDefaultTokenProviders();

            var jwtSection = builder.Configuration.GetSection("JWT");
            builder.Services.Configure<JwtSetting>(jwtSection);

            var jwtSetting = jwtSection.Get<JwtSetting>();
            if (jwtSetting == null || string.IsNullOrEmpty(jwtSetting.SIGNINGKEY))
                throw new InvalidOperationException("JWT settings are not properly configured.");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // Enforce HTTPS in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSetting.ISSUER,
                    ValidAudience = jwtSetting.AUDIENCE,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SIGNINGKEY))
                };
            });
            #endregion

            #region Manager Setting registeration:
            var managerSetting = builder.Configuration.GetSection("MANAGER");
            builder.Services.Configure<ManagerSetting>(managerSetting);
            #endregion

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            #region Swagger Setting and Documentation:
            ConfigureSwaggerServices(builder.Services);
            #endregion

            #region Permission Registration (Authorization policies):
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ManagerPolicy", builder => builder.RequireRole("Manager"));
            });
            #endregion


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            #region Seed Data (Roles, Users)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                SeedDataAsync(services, builder.Configuration).GetAwaiter().GetResult();
            }
            #endregion


            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        /// <summary>
        /// Asynchronously seeds the database with initial data, including roles and users.
        /// This method retrieves the necessary services from the provided service provider,
        /// and attempts to seed default roles, a manager user, and permissions. 
        /// It logs the outcome of the seeding process, including any exceptions that may occur.
        /// </summary>
        /// <param name="services">The service provider containing the required services for seeding.</param>
        /// <param name="configuration">The application configuration used to access settings.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task SeedDataAsync(IServiceProvider services, IConfiguration configuration)
        {
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("app");

            try
            {
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<RoutingServiceAppUser>>();


                // Perform role and user seeding
                await SeedRoles.SeedRolesAsync(roleManager);
                await SeedUsers.SeedManagerUserAsync(userManager, configuration, roleManager);

                logger.LogInformation("Finished Seeding Default Data");
                logger.LogInformation("Application Starting");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "An error occurred seeding the DB");
            }
        }

        /// <summary>
        /// Configures Swagger services for API documentation and testing.
        /// This method sets up the Swagger generator with metadata about the API,
        /// including its title, version, description, and contact information.
        /// It also defines a security scheme for JWT authentication, allowing users
        /// to provide a Bearer token in the request headers for authorized access.
        /// </summary>
        /// <param name="services">The service collection to which Swagger services will be added.</param>
        private static void ConfigureSwaggerServices(IServiceCollection services)
        {
            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Version = "v1",
                    Title = "Routing Service API",
                    Description = "ROUTING SERVICE API HELPS YOU TO FIND THE SHORTEST PATH BETWEEN GIVEN TWO POINTS, THROUGH UPLOADING CSV OR TXT FILES.",
                    Contact = new OpenApiContact()
                    {
                        Name = "Wahab Alhamad",
                        Email = "alhamadwahab@gmail.com",
                        Url = new Uri("https://codepen.io/dev70")
                    }
                });

                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT Key"
                });

                s.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });
        }
    }
}