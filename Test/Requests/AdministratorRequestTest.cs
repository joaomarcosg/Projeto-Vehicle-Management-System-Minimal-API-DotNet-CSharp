using System.Net;
using System.Text;
using System.Text.Json;
using MinimalApi.Domain.ModelViews;
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

        var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");

        var response = await Setup.client.PostAsync("/administrators/login", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        var loggedInAdm = JsonSerializer.Deserialize<LoggedInAdministrator>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(loggedInAdm?.Email ?? "");
        Assert.IsNotNull(loggedInAdm?.Profile ?? "");
        Assert.IsNotNull(loggedInAdm?.Token ?? "");

    }
}