using System.Net;
using System.Net.Http.Json;
using DonaldsonMotors.API.DTOs.Part;
using DonaldsonMotors.API.DTOs.Schedule;
using DonaldsonMotors.API.DTOs.ServiceType;
using DonaldsonMotors.API.DTOs.Supplier;

namespace DonaldsonMotors.Tests.Integration;

/// <summary>Suppliers, parts, service types and the garage schedule.</summary>
[Collection(ApiCollection.Name)]
public class CatalogueTests(ApiFactory app)
{
    [Fact]
    public async Task Suppliers_CanBeAddedRenamedAndRemoved()
    {
        var stock = await app.ClientForAsync(DemoAccounts.StockController);

        var created = await stock.PostAsJsonAsync("/api/suppliers", new { name = "Brake World", postcode = "G3 3CC", email = "sales@brakeworld.example" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var supplier = (await created.Content.ReadFromJsonAsync<SupplierResponseDto>())!;

        var renamed = await stock.PutAsJsonAsync($"/api/suppliers/{supplier.Id}", new { name = "Brake World Ltd" });
        Assert.Equal(HttpStatusCode.OK, renamed.StatusCode);
        Assert.Equal("Brake World Ltd", (await stock.GetFromJsonAsync<SupplierResponseDto>($"/api/suppliers/{supplier.Id}"))!.Name);

        Assert.Equal(HttpStatusCode.NoContent, (await stock.DeleteAsync($"/api/suppliers/{supplier.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await stock.GetAsync($"/api/suppliers/{supplier.Id}")).StatusCode);
    }

    [Fact]
    public async Task Suppliers_MustHaveUniqueNames()
    {
        var stock = await app.ClientForAsync(DemoAccounts.StockController);

        var response = await stock.PostAsJsonAsync("/api/suppliers", new { name = "Euro Car Parts", postcode = "G1 1AA" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ASupplierWithPartsCannotBeRemoved()
    {
        var stock = await app.ClientForAsync(DemoAccounts.StockController);
        var suppliers = await stock.GetFromJsonAsync<List<SupplierResponseDto>>("/api/suppliers");
        var euroParts = suppliers!.Single(s => s.Name == "Euro Car Parts");

        var response = await stock.DeleteAsync($"/api/suppliers/{euroParts.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Parts_CanBeSearchedEditedAndRemoved()
    {
        var stock = await app.ClientForAsync(DemoAccounts.StockController);
        var created = await stock.PostAsJsonAsync("/api/parts", new { name = "Headlight bulb H7", price = 9.99m, costPrice = 4m, initialStockLevel = 12, barcode = "H7-55W", supplierId = 1 });
        var part = (await created.Content.ReadFromJsonAsync<PartResponseDto>())!;

        var found = await stock.GetFromJsonAsync<List<PartResponseDto>>("/api/parts?searchTerm=headlight");
        Assert.Contains(found!, p => p.Id == part.Id);

        var edited = await stock.PutAsJsonAsync($"/api/parts/{part.Id}", new { price = 11.49m });
        Assert.Equal(11.49m, (await edited.Content.ReadFromJsonAsync<PartResponseDto>())!.Price);

        Assert.Equal(HttpStatusCode.NoContent, (await stock.DeleteAsync($"/api/parts/{part.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await stock.GetAsync($"/api/parts/{part.Id}")).StatusCode);
    }

    [Fact]
    public async Task APartUsedOnAJobCannotBeRemoved()
    {
        // The demo data has finished jobs that used the oil filter
        var stock = await app.ClientForAsync(DemoAccounts.StockController);
        var filter = (await stock.GetFromJsonAsync<List<PartResponseDto>>("/api/parts"))!.Single(p => p.Name == "Oil Filter Bosch H1");

        var response = await stock.DeleteAsync($"/api/parts/{filter.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Managers_CanManageServiceTypes()
    {
        var manager = await app.ClientForAsync(DemoAccounts.Manager);

        var created = await manager.PostAsJsonAsync("/api/servicetypes", new { name = "Air Con Regas", description = "Recharge the air conditioning.", price = 60m, durationHours = 1.0 });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var service = (await created.Content.ReadFromJsonAsync<ServiceTypeResponseDto>())!;

        var updated = await manager.PutAsJsonAsync($"/api/servicetypes/{service.Id}", new { price = 65m });
        Assert.True(updated.IsSuccessStatusCode);
        Assert.Contains(await app.CreateClient().GetFromJsonAsync<List<ServiceTypeResponseDto>>("/api/servicetypes") ?? [],
            s => s.Id == service.Id && s.Price == 65m);

        Assert.Equal(HttpStatusCode.NoContent, (await manager.DeleteAsync($"/api/servicetypes/{service.Id}")).StatusCode);
    }

    [Fact]
    public async Task ServiceTypes_WithBookingsCannotBeRemoved_AndCustomersCannotEditThem()
    {
        var manager = await app.ClientForAsync(DemoAccounts.Manager);
        var customer = await app.ClientForAsync(DemoAccounts.Customer);

        Assert.Equal(HttpStatusCode.Forbidden, (await customer.PostAsJsonAsync("/api/servicetypes", new { name = "Free service", description = "x", price = 1m, durationHours = 1.0 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await manager.DeleteAsync("/api/servicetypes/1")).StatusCode);
    }

    [Fact]
    public async Task AHolidayRemovesThatDayFromAvailability()
    {
        var manager = await app.ClientForAsync(DemoAccounts.Manager);
        var holiday = new DateTime(2031, 3, 3, 0, 0, 0, DateTimeKind.Utc); // a Monday

        var created = await manager.PostAsJsonAsync("/api/schedule/exceptions", new { date = holiday, description = "Staff training" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var exception = (await created.Content.ReadFromJsonAsync<ScheduleExceptionResponseDto>())!;

        var slots = await app.CreateClient().GetFromJsonAsync<List<DateTime>>("/api/schedule/availability?startDate=2031-03-03&endDate=2031-03-04");
        Assert.DoesNotContain(slots!, s => s.Date == holiday.Date);
        Assert.Contains(await manager.GetFromJsonAsync<List<ScheduleExceptionResponseDto>>("/api/schedule/exceptions") ?? [], e => e.Id == exception.Id);

        Assert.Equal(HttpStatusCode.NoContent, (await manager.DeleteAsync($"/api/schedule/exceptions/{exception.Id}")).StatusCode);
        slots = await app.CreateClient().GetFromJsonAsync<List<DateTime>>("/api/schedule/availability?startDate=2031-03-03&endDate=2031-03-04");
        Assert.Contains(slots!, s => s.Date == holiday.Date);
    }

    [Fact]
    public async Task AvailabilityRejectsABadDateRange()
    {
        var client = app.CreateClient();

        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/schedule/availability?startDate=2031-03-10&endDate=2031-03-01")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/schedule/availability?startDate=2031-01-01&endDate=2031-06-01")).StatusCode);
    }
}
