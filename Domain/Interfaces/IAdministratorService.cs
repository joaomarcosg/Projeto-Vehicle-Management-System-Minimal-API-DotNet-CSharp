using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;

namespace MinimalApi.Domain.Interfaces;

public interface IAdministratorService
{
    Administrator? Login(LoginDTO loginDTO);
}