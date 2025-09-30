using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;

namespace MinimalApi.Domain.Interfaces;

public interface IAdministratorService
{
    Administrator? Login(LoginDTO loginDTO);
    Administrator Add(Administrator administrator);
    List<Administrator> ListAdministrators(int? page);
}