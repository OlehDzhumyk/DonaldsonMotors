using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Add MVC + Swagger
builder.Services.AddControllers();
// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region Configure Database (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
    // Optional: specify MigrationsAssembly if your migrations live elsewhere:
    // , npgsqlOptions => npgsqlOptions.MigrationsAssembly("DonaldsonMotors.API")
    )
);
#endregion

#region Register Repositories (DI)
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
#endregion

#region Configure Kestrel Endpoints
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP on port 80 (mapped in docker-compose to 5000:80)
    options.ListenAnyIP(80);

    // HTTPS on port 8081 (mapped to 5001:8081), mounts cert via env vars
    // options.ListenAnyIP(8081, listenOptions => listenOptions.UseHttps());
});
#endregion

var app = builder.Build();

#region Auto‐Apply EF Migrations on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        var pending = await dbContext.Database.GetPendingMigrationsAsync();

        if (pending.Any())
        {
            Console.WriteLine("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("Database migrations applied successfully.");
        }
        else
        {
            Console.WriteLine("No pending migrations to apply.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        // decide: rethrow or swallow depending on your policy
    }
}
#endregion

#region HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
#endregion

app.Run();
