using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infrastructure.Db;

namespace MinimalApi.Domain.Services;

public class VehicleService : IVehicleService
{
    private readonly DataBaseContext _context;
    public VehicleService(DataBaseContext context)
    {
        _context = context;
    }
    public void AddVehicle(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();
    }

    public void DeleteVehicle(Vehicle vehicle)
    {
        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
    }

    public List<Vehicle> ListVehicles(int? page = 1, string? name = null, string? mark = null)
    {
        var query = _context.Vehicles.AsQueryable();
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(vehicle => EF.Functions.Like(vehicle.Name.ToLower(), $"%{name}%"));
        }

        int itemsPerPage = 10;

        if (page != null)
        {
            query = query.Skip(((int)page - 1) * itemsPerPage).Take(itemsPerPage);
        }
        

        return query.ToList();
    }

    public Vehicle? SeachById(int id)
    {
        return _context.Vehicles.Where(vehicle => vehicle.Id == id).FirstOrDefault();
    }

    public void UpdateVehicle(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        _context.SaveChanges();
    }
}