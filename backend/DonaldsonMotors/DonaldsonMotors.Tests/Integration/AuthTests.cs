using System.Net;
using System.Net.Http.Json;
using DonaldsonMotors.API.DTOs.Auth;

namespace DonaldsonMotors.Tests.Integration;

[Collection(ApiCollection.Name)]
public class AuthTests(ApiFactory app)
{
    private static object NewUser(string email, string? role = null) =>
        new { fullName = "Test Person", email, password = "secret12", role };

    [Fact]
    public async Task Register_CreatesACustomerAndReturnsAToken()
    {
        var response = await app.CreateClient().PostAsJsonAsync("/api/auth/register", NewUser("new.customer@example.com"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.Equal("Customer", auth!.Role);
        Assert.False(string.IsNullOrEmpty(auth.Token));
    }

    [Fact]
    public async Task Register_IgnoresARoleSentByTheClient()
    {
        var response = await app.CreateClient().PostAsJsonAsync("/api/auth/register", NewUser("wants.to.be.manager@example.com", "Manager"));

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.Equal("Customer", auth!.Role);
    }

    [Fact]
    public async Task Register_RejectsAnEmailThatIsAlreadyUsed()
    {
        var response = await app.CreateClient().PostAsJsonAsync("/api/auth/register", NewUser(DemoAccounts.Customer));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_RejectsAWrongPassword()
    {
        var response = await app.CreateClient().PostAsJsonAsync("/api/auth/login", new { email = DemoAccounts.Manager, password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterStaff_IsOnlyForManagers()
    {
        var staff = NewUser("new.mechanic@donaldson.com", "Mechanic");

        var anonymous = await app.CreateClient().PostAsJsonAsync("/api/auth/register/staff", staff);
        var customer = await (await app.ClientForAsync(DemoAccounts.Customer)).PostAsJsonAsync("/api/auth/register/staff", staff);
        var manager = await (await app.ClientForAsync(DemoAccounts.Manager)).PostAsJsonAsync("/api/auth/register/staff", staff);

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, customer.StatusCode);
        Assert.Equal(HttpStatusCode.OK, manager.StatusCode);
        Assert.Equal("Mechanic", (await manager.Content.ReadFromJsonAsync<AuthResponseDto>())!.Role);
    }

    [Fact]
    public async Task ProtectedEndpoints_RejectRequestsWithoutAToken()
    {
        var response = await app.CreateClient().GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
