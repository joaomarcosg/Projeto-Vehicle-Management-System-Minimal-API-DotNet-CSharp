using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infrastructure.Db;

namespace MinimalApi.Domain.Services;

public class VehicleService : IVehicleService
{
    private readonly DataBaseContext _context;
    public Vehicle AddVehicle(Vehicle vehicle)
    {
        throw new NotImplementedException();
    }

    public void DeleteVehicle(Vehicle vehicle)
    {
        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
    }

    public List<Vehicle> ListVehicles(int page = 1, string? name = null, string? mark = null)
    {
        throw new NotImplementedException();
    }

    public Vehicle SeachById(int id)
    {
        throw new NotImplementedException();
    }

    public void UpdateVehicle(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        _context.SaveChanges();
    }
}