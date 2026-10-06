using System.Net;
using System.Net.Http.Json;
using DonaldsonMotors.API.DTOs.Part;

namespace DonaldsonMotors.Tests.Integration;

/// <summary>Who may do what with stock, suppliers and the schedule.</summary>
[Collection(ApiCollection.Name)]
public class PermissionTests(ApiFactory app)
{
    private static object NewPart(string name) =>
        new { name, price = 12.50m, costPrice = 8m, initialStockLevel = 5, supplierId = 1 };

    [Fact]
    public async Task Mechanics_CanReadPartsButNotChangeThem()
    {
        var mechanic = await app.ClientForAsync(DemoAccounts.Mechanic);
        var parts = await mechanic.GetFromJsonAsync<List<PartResponseDto>>("/api/parts");
        var partId = parts![0].Id;

        Assert.Equal(HttpStatusCode.Forbidden, (await mechanic.PostAsJsonAsync("/api/parts", NewPart("Wiper blade"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await mechanic.PatchAsJsonAsync($"/api/parts/{partId}/stock", new { changeInQuantity = 100 })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await mechanic.DeleteAsync($"/api/parts/{partId}")).StatusCode);
    }

    [Fact]
    public async Task StockControllers_CanAddPartsAndChangeStock()
    {
        var stock = await app.ClientForAsync(DemoAccounts.StockController);

        var created = await stock.PostAsJsonAsync("/api/parts", NewPart("Cabin air filter"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var part = await created.Content.ReadFromJsonAsync<PartResponseDto>();

        var restocked = await stock.PatchAsJsonAsync($"/api/parts/{part!.Id}/stock", new { changeInQuantity = 7, reason = "Delivery" });
        Assert.Equal(12, (await restocked.Content.ReadFromJsonAsync<PartResponseDto>())!.CurrentStockLevel);
    }

    [Fact]
    public async Task Stock_CannotGoBelowZero()
    {
        var stock = await app.ClientForAsync(DemoAccounts.StockController);
        var part = await (await stock.PostAsJsonAsync("/api/parts", NewPart("Fuse set"))).Content.ReadFromJsonAsync<PartResponseDto>();

        var response = await stock.PatchAsJsonAsync($"/api/parts/{part!.Id}/stock", new { changeInQuantity = -6 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Customers_CannotSeeStockOrSuppliers()
    {
        var customer = await app.ClientForAsync(DemoAccounts.Customer);

        Assert.Equal(HttpStatusCode.Forbidden, (await customer.GetAsync("/api/parts")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.GetAsync("/api/suppliers")).StatusCode);
    }

    [Fact]
    public async Task OnlyManagers_CanChangeWorkingHours()
    {
        var hours = new[] { new { dayOfWeek = 6, startTime = "09:00", endTime = "12:00" } };

        var mechanic = await (await app.ClientForAsync(DemoAccounts.Mechanic)).PutAsJsonAsync("/api/schedule/working-hours", hours);

        Assert.Equal(HttpStatusCode.Forbidden, mechanic.StatusCode);
    }

    [Fact]
    public async Task ServicesAndAvailability_ArePublic()
    {
        var client = app.CreateClient();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var nextWeek = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/servicetypes")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/schedule/availability?startDate={today}&endDate={nextWeek}")).StatusCode);
    }
}
