using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Enums;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();

if (string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataBaseContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("Mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
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
    };
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
app.MapPost("/administrators/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
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
}).WithTags("Administrator");

app.MapGet("/administrators", ([FromQuery] int? page, IAdministratorService administratorService) =>
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

}).RequireAuthorization().WithTags("Administrator");

app.MapGet("/administrators/{id}", ([FromRoute] int id, IAdministratorService administratorService) =>
{
    var administrator = administratorService.SearchById(id);

    if (administrator == null) return Results.NotFound();

    return Results.Ok(new AdministratorModelView
        {
            Id = administrator.Id,
            Email = administrator.Email,
            Profile = administrator.Profile
        });

}).RequireAuthorization().WithTags("Administrator");

app.MapPost("/administrators", ([FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) =>
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

}).RequireAuthorization().WithTags("Administrator");
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

app.MapPost("/vehicle", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
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

}).RequireAuthorization().WithTags("Vehicle");

app.MapGet("/vehicle", ([FromQuery] int? page, IVehicleService vehicleService) =>
{
    var vehicles = vehicleService.ListVehicles(page);

    return Results.Ok(vehicles);
}).WithTags("Vehicle");

app.MapGet("/vehicle/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.SearchById(id);

    if (vehicle == null) return Results.NotFound();

    return Results.Ok(vehicle);

}).RequireAuthorization().WithTags("Vehicle");

app.MapPut("/vehicle/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
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

}).RequireAuthorization().WithTags("Vehicle");

app.MapDelete("/vehicle/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.SearchById(id);

    if (vehicle == null) return Results.NotFound();

    vehicleService.DeleteVehicle(vehicle);

    return Results.NoContent();

}).RequireAuthorization().WithTags("Vehicle");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion