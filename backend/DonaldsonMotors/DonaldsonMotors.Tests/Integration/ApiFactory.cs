using System.Net.Http.Headers;
using System.Net.Http.Json;
using DonaldsonMotors.API.DTOs.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace DonaldsonMotors.Tests.Integration;

/// <summary>
/// Runs the real API against a throwaway PostgreSQL container. On start-up the app applies
/// the migrations and seeds the demo data, exactly as it does in Docker Compose.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string Password = "Password123!";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:Key", "integration-tests-only-signing-key-0123456789abcdef");
        builder.UseSetting("Jwt:Issuer", "donaldson-tests");
        builder.UseSetting("Jwt:Audience", "donaldson-tests");
        builder.UseSetting("Jwt:ExpiryMinutes", "60");
        builder.UseSetting("SeedDemoData", "true");
        builder.UseSetting("EmailSettings:SmtpHost", "");
        builder.UseSetting("Logging:LogLevel:Default", "Warning");
    }

    /// <summary>A client logged in as one of the seeded demo accounts.</summary>
    public async Task<HttpClient> ClientForAsync(string email)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "API";
}

public static class DemoAccounts
{
    public const string Manager = "manager@donaldson.com";
    public const string Mechanic = "mechanic@donaldson.com";
    public const string OtherMechanic = "lee@donaldson.com";
    public const string StockController = "stock@donaldson.com";
    public const string AccountsClerk = "accounts@donaldson.com";
    public const string Customer = "customer@donaldson.com";
    public const string OtherCustomer = "priya@example.com";
}
