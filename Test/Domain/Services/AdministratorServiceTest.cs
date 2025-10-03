using MinimalApi.Domain.Entities;

namespace Test.Domain.Services;

[TestClass]
public class AdministratorTest
{
    [TestMethod]
    public void TestSaveAdministrator()
    {
        var adm = new Administrator();

        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Password = "teste";
        adm.Profile = "Adm";

        
    }
}