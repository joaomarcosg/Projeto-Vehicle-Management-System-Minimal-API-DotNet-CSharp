using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Enums;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration?.GetSection("Jwt")?.ToString() ?? "";
    }

    private string key;
    public IConfiguration Configuration { get; set; } = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option =>
        {
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministratorService, AdministratorService>();
        services.AddScoped<IVehicleService, VehicleService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insert JWT token here"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                new string[] {}
                }
            });
        });

        services.AddDbContext<DataBaseContext>(options =>
        {
            options.UseMySql(
            Configuration.GetConnectionString("Mysql"),
            ServerVersion.AutoDetect(Configuration.GetConnectionString("mysql"))
            );
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
            #endregion

            #region Administrators
            string GenerateJwtToken(Administrator administrator)
            {
                if (string.IsNullOrEmpty(key)) return string.Empty;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new Claim("Email", administrator.Email),
                    new Claim("Profile", administrator.Profile),
                    new Claim(ClaimTypes.Role, administrator.Profile),
                };
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            endpoints.MapPost("/administrators/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
            {
                var adm = administratorService.Login(loginDTO);
                if (adm != null)
                {
                    string token = GenerateJwtToken(adm);
                    return Results.Ok(new LoggedInAdministrator
                    {
                        Email = adm.Email,
                        Profile = adm.Profile,
                        Token = token
                    });
                }
                else
                return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Administrator");

            endpoints.MapGet("/administrators", ([FromQuery] int? page, IAdministratorService administratorService) =>
            {
                var adms = new List<AdministratorModelView>();
                var administrators = administratorService.ListAdministrators(page);
                foreach (var adm in administrators)
                {
                    adms.Add(new AdministratorModelView
                    {
                        Id = adm.Id,
                        Email = adm.Email,
                        Profile = adm.Profile
                    });
                }

                return Results.Ok(adms);

            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm"}).WithTags("Administrator");

            endpoints.MapGet("/administrators/{id}", ([FromRoute] int id, IAdministratorService administratorService) =>
            {
                var administrator = administratorService.SearchById(id);

                if (administrator == null) return Results.NotFound();

                return Results.Ok(new AdministratorModelView
                {
                    Id = administrator.Id,
                    Email = administrator.Email,
                    Profile = administrator.Profile
                });

            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm"}).WithTags("Administrator");

            endpoints.MapPost("/administrators", ([FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) =>
            {
                var validation = new ValidationErrors
                {
                    Messages = new List<string>()
                };

                if (string.IsNullOrEmpty(administratorDTO.Email))
                    validation.Messages.Add("Email cannot be empty");

                if (string.IsNullOrEmpty(administratorDTO.Password))
                    validation.Messages.Add("Password cannot be empty");

                if (administratorDTO.Profile == null)
                    validation.Messages.Add("Profile cannot be empty");

                if (validation.Messages.Count > 0)
                    return Results.BadRequest(validation);


                var administrator = new Administrator
                {
                    Email = administratorDTO.Email,
                    Password = administratorDTO.Password,
                    Profile = administratorDTO.Profile.ToString() ?? Profile.Editor.ToString()
                };

                administratorService.Add(administrator);

                return Results.Created($"/administrators/{administrator.Id}", new AdministratorModelView
                {
                    Id = administrator.Id,
                    Email = administrator.Email,
                    Profile = administrator.Profile
                });

            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm"}).WithTags("Administrator");
            #endregion

            #region Vehicles
            ValidationErrors validateDTO(VehicleDTO vehicleDTO)
            {
                var validation = new ValidationErrors
                {
                    Messages = new List<string>()
                };

                if (string.IsNullOrEmpty(vehicleDTO.Name))
                    validation.Messages.Add("Name cannot be empty");

                if (string.IsNullOrEmpty(vehicleDTO.Mark))
                    validation.Messages.Add("Mark cannot be empty");

                if (vehicleDTO.Year <= 1950)
                    validation.Messages.Add("Very old vehicle, only year after 1950");
                
                return validation;
            }

            endpoints.MapPost("/vehicle", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
            {

                var validation = validateDTO(vehicleDTO);

                if (validation.Messages.Count > 0)
                    return Results.BadRequest(validation);

                var vehicle = new Vehicle
                {
                    Name = vehicleDTO.Name,
                    Mark = vehicleDTO.Mark,
                    Year = vehicleDTO.Year
                };

                vehicleService.AddVehicle(vehicle);

                return Results.Created($"/vehicle/{vehicle.Id}", vehicle);

            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Vehicle");

            endpoints.MapGet("/vehicle", ([FromQuery] int? page, IVehicleService vehicleService) =>
            {
                var vehicles = vehicleService.ListVehicles(page);

                return Results.Ok(vehicles);
            }).WithTags("Vehicle");

            endpoints.MapGet("/vehicle/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
            {
                var vehicle = vehicleService.SearchById(id);

                if (vehicle == null) return Results.NotFound();

                 return Results.Ok(vehicle);

            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Vehicle");

            endpoints.MapPut("/vehicle/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
            {
                var vehicle = vehicleService.SearchById(id);

                if (vehicle == null) return Results.NotFound();

                var validation = validateDTO(vehicleDTO);

                if (validation.Messages.Count > 0)
                    return Results.BadRequest(validation);

                vehicle.Name = vehicleDTO.Name;
                vehicle.Mark = vehicleDTO.Mark;
                vehicle.Year = vehicleDTO.Year;

                vehicleService.UpdateVehicle(vehicle);

                return Results.Ok(vehicle);

            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Vehicle");

            endpoints.MapDelete("/vehicle/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
            {
                var vehicle = vehicleService.SearchById(id);

                if (vehicle == null) return Results.NotFound();

                vehicleService.DeleteVehicle(vehicle);

                return Results.NoContent();

            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Vehicle");
            #endregion
        });
    }
}