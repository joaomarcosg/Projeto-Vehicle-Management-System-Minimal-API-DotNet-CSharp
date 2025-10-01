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

    public Administrator Add(Administrator administrator)
    {
        _context.Administrators.Add(administrator);
        _context.SaveChanges();
        return administrator;
    }

    public Administrator? SearchById(int id)
    {
        return _context.Administrators.Where(a => a.Id == id).FirstOrDefault();
    }

    public List<Administrator> ListAdministrators(int? page)
    {
        var query = _context.Administrators.AsQueryable();

        int itemsPerPage = 10;

        if (page != null)
        {
            query = query.Skip(((int)page - 1) * itemsPerPage).Take(itemsPerPage);
        }

        return query.ToList();
    }

    public Administrator? Login(LoginDTO loginDTO)
    {
        var adm = _context.Administrators.Where(
            a => a.Email == loginDTO.Email && a.Password == loginDTO.Password
        ).FirstOrDefault();
        
        return adm;
    }
}