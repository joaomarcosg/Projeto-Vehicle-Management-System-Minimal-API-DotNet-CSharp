using MinimalApi.Domain.Entities;

namespace MinimalApi.Domain.Interfaces;

public interface IVehicleService
{
    List<Vehicle> ListVehicles(int page = 1, string? name = null, string? mark = null);
    Vehicle SeachById(int id);
    Vehicle AddVehicle(Vehicle vehicle);
    Vehicle UpdateVehicle(Vehicle vehicle);
    Vehicle DeleteVehicle(Vehicle vehicle);
}