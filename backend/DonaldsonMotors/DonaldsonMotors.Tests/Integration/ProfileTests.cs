using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DonaldsonMotors.API.DTOs.Auth;
using DonaldsonMotors.API.DTOs.User;

namespace DonaldsonMotors.Tests.Integration;

/// <summary>Customers managing their own details and vehicles.</summary>
[Collection(ApiCollection.Name)]
public class ProfileTests(ApiFactory app)
{
    /// <summary>A brand-new customer, so changes here never affect the shared demo accounts.</summary>
    private async Task<HttpClient> NewCustomerAsync(string email)
    {
        var client = app.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new { fullName = "Profile Test", email, password = "secret12" });
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    private static object Car(string reg, int mileage = 1000) =>
        new { registrationNumber = reg, make = "Skoda", model = "Octavia", year = 2019, mileage };

    [Fact]
    public async Task ACustomerCanUpdateTheirDetails()
    {
        var client = await NewCustomerAsync("details@example.com");

        var response = await client.PutAsJsonAsync("/api/users/me", new { fullName = "Updated Name", address = "2 New Road", telephoneNumber = "07700 900999" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await client.GetFromJsonAsync<UserProfileResponseDto>("/api/users/me");
        Assert.Equal("Updated Name", profile!.FullName);
        Assert.Equal("2 New Road", profile.Address);
        Assert.Equal("07700 900999", profile.PhoneNumber);
    }

    [Fact]
    public async Task ACustomerCanAddEditAndRemoveAVehicle()
    {
        var client = await NewCustomerAsync("garage@example.com");

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/users/me/vehicles", Car("PR01TST"))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync("/api/users/me/vehicles/PR01TST", Car("PR01TST", 2500))).StatusCode);
        var profile = await client.GetFromJsonAsync<UserProfileResponseDto>("/api/users/me");
        Assert.Equal(2500, Assert.Single(profile!.Vehicles).Mileage);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/api/users/me/vehicles/PR01TST")).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<UserProfileResponseDto>("/api/users/me"))!.Vehicles);
    }

    [Fact]
    public async Task ARegistrationCanOnlyBeAddedOnce()
    {
        var client = await NewCustomerAsync("duplicate.car@example.com");

        var response = await client.PostAsJsonAsync("/api/users/me/vehicles", Car("SG21ABC"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ACustomerCannotChangeOrRemoveSomeoneElsesVehicle()
    {
        var client = await NewCustomerAsync("nosy@example.com");

        Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsJsonAsync("/api/users/me/vehicles/SG21ABC", Car("SG21ABC"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync("/api/users/me/vehicles/SG21ABC")).StatusCode);
    }

    [Fact]
    public async Task AVehicleWithAnUpcomingBookingCannotBeRemoved()
    {
        // SG21ABC has a pending booking in the demo data
        var customer = await app.ClientForAsync(DemoAccounts.Customer);

        var response = await customer.DeleteAsync("/api/users/me/vehicles/SG21ABC");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ACustomerCanChangeTheirPassword()
    {
        var client = await NewCustomerAsync("new.password@example.com");

        var wrong = await client.PostAsJsonAsync("/api/users/me/change-password", new { currentPassword = "not-it", newPassword = "better12", confirmNewPassword = "better12" });
        var right = await client.PostAsJsonAsync("/api/users/me/change-password", new { currentPassword = "secret12", newPassword = "better12", confirmNewPassword = "better12" });

        Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);
        Assert.True(right.IsSuccessStatusCode);
        var login = await app.CreateClient().PostAsJsonAsync("/api/auth/login", new { email = "new.password@example.com", password = "better12" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task AManagerCanEditAnyUser_ButACustomerCannot()
    {
        var newCustomer = await NewCustomerAsync("edited.by.manager@example.com");
        var id = (await newCustomer.GetFromJsonAsync<UserProfileResponseDto>("/api/users/me"))!.Id;
        var manager = await app.ClientForAsync(DemoAccounts.Manager);
        var customer = await app.ClientForAsync(DemoAccounts.Customer);
        var change = new { fullName = "Renamed By Manager" };

        Assert.Equal(HttpStatusCode.Forbidden, (await customer.PutAsJsonAsync($"/api/users/{id}", change)).StatusCode);
        var response = await manager.PutAsJsonAsync($"/api/users/{id}", change);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Renamed By Manager", (await response.Content.ReadFromJsonAsync<UserProfileResponseDto>())!.FullName);
    }
}
