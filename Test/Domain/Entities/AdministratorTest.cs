using MinimalApi.Domain.Entities;

namespace Test.Domain.Entities;

[TestClass]
public class AdministratorTest
{
    [TestMethod]
    public void TestGetAndSetProperties()
    {
        var adm = new Administrator();

        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Password = "teste";
        adm.Profile = "Adm";

        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("teste", adm.Password);
        Assert.AreEqual("Adm", adm.Profile);
    }
}