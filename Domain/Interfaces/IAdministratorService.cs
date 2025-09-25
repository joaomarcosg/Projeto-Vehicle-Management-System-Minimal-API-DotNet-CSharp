using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;

namespace MinimalApi.Domain.Interfaces;

public interface IAdministratorService
{
    List<Administrator> Login(LoginDTO loginDTO);
}