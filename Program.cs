using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Enums;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataBaseContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administrators
app.MapPost("/administrators/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
{
    if (administratorService.Login(loginDTO) != null)
        return Results.Ok("Logged in sucessfully");
    else
        return Results.Unauthorized();
}).WithTags("Administrator");

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
        Profile = administratorDTO.Profile.ToString() ?? Profile.editor.ToString()
    };

    return Results.Created();

}).WithTags("Administrator");
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

}).WithTags("Vehicle");

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

}).WithTags("Vehicle");

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

}).WithTags("Vehicle");

app.MapDelete("/vehicle/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.SearchById(id);

    if (vehicle == null) return Results.NotFound();

    vehicleService.DeleteVehicle(vehicle);

    return Results.NoContent();

}).WithTags("Vehicle");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion