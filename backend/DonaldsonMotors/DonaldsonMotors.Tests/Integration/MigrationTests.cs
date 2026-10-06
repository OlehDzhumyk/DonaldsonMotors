using DonaldsonMotors.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DonaldsonMotors.Tests.Integration;

[Collection(ApiCollection.Name)]
public class MigrationTests(ApiFactory app)
{
    [Fact]
    public async Task TheMigrationsMatchTheModel()
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.False(db.Database.HasPendingModelChanges(), "The model has changes that are not in a migration. Run dotnet ef migrations add.");
    }
}
