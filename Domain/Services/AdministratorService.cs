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
    public bool Login(LoginDTO loginDTO)
    {
        var qtd = _context.Administrators.Where(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password).Count();
        return qtd > 0;
    }
}