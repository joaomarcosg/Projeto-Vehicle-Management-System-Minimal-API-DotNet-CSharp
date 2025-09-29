using MinimalApi.Domain.Entities;

namespace MinimalApi.Domain.Interfaces;

public interface IVehicleService
{
    List<Vehicle> ListVehicles(int? page = 1, string? name = null, string? mark = null);
    Vehicle? SeachById(int id);
    void AddVehicle(Vehicle vehicle);
    void UpdateVehicle(Vehicle vehicle);
    void DeleteVehicle(Vehicle vehicle);
}