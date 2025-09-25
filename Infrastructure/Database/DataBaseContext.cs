using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;

namespace MinimalApi.Infrastructure.Db;

public class DataBaseContext : DbContext
{
    private readonly IConfiguration _configurationAppSettings;
    public DataBaseContext(IConfiguration configurationAppSettings)
    {
        _configurationAppSettings = configurationAppSettings;
    }
    public DbSet<Administrator> Administrators { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configurationAppSettings.GetConnectionString("mysql")?.ToString();
        if (!string.IsNullOrEmpty(connectionString))
        {
            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );
        }


    }
}