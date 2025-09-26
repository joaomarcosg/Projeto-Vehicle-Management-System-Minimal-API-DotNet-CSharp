using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;

namespace MinimalApi.Domain.Services;

public class AdministratorService : IAdministratorService
{
    private readonly DataBaseContext _context;
    public AdministratorService(DataBaseContext context)
    {
        _context = context;
    }
    public Administrator? Login(LoginDTO loginDTO)
    {
        var adm = _context.Administrators.Where(
            a => a.Email == loginDTO.Email && a.Password == loginDTO.Password
        ).FirstOrDefault();
        
        return adm;
    }
}