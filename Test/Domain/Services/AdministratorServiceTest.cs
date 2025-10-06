using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Services;
using MinimalApi.Infrastructure.Db;

namespace Test.Domain.Services;

[TestClass]
public class AdministratorServiceTest
{
    private DbContext CreateTestContext()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var builder = new ConfigurationBuilder()
            .SetBasePath(path ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();
        return new DataBaseContext(configuration);
    }
    [TestMethod]
    public void TestSaveAdministrator()
    {
        var context = CreateTestContext();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE administrators");

        var adm = new Administrator();
        adm.Email = "teste@teste.com";
        adm.Password = "teste";
        adm.Profile = "Adm";
        var administratorService = new AdministratorService((DataBaseContext)context);

        administratorService.Add(adm);

        Assert.AreEqual(1, administratorService.ListAdministrators(1).Count());
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("teste", adm.Password);
        Assert.AreEqual("Adm", adm.Profile);


    }

        public void TestSearchAdministratorById()
    {   
        var context = CreateTestContext();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE administrators");

        var adm = new Administrator();
        adm.Email = "teste@teste.com";
        adm.Password = "teste";
        adm.Profile = "Adm";
        var administratorService = new AdministratorService((DataBaseContext)context);

        administratorService.Add(adm);
        var admDatabase = administratorService.SearchById(adm.Id);

        Assert.AreEqual(1, admDatabase.Id);
        
    }
}