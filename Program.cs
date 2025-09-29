using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;
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
#endregion

#region Vehicles
app.MapPost("/vehicle", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
{
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