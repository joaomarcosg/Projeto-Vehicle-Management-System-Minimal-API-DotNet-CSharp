using System.Text;
using System.Text.Json;
using MinimalApi.DTOs;
using Test.Helpers;

namespace Test.Requests;

[TestClass]
public class AdministratorRequestTest
{
    [ClassInitialize]
    public static void ClassInt(TestContext testContext)
    {
        Setup.ClassInit(testContext);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Setup.ClassCleanup();
    }

    [TestMethod]
    public async Task TestGetAndSetProperties()
    {
        var loginDTO = new LoginDTO
        {
            Email = "adm@teste.com",
            Password = "123456"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json");

    }
}