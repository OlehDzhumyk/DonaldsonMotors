using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Repositories;
using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.Services;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Logers;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Logging.AddFilter("Microsoft.AspNetCore.Authorization", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Identity", LogLevel.Debug);

#region — Конфігурація JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtExpiryMin = int.Parse(builder.Configuration["Jwt:ExpiryMinutes"] ?? "60");
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
#endregion

#region — Сервіси
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
{
    opts.User.RequireUniqueEmail = true;
    opts.Password.RequiredLength = 6;
    opts.Password.RequireDigit = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "https://localhost:5000", // твій домен
        ValidAudience = "https://localhost:5000",
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)

    };
});

builder.Services.AddAuthorization();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();


// Kestrel
builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(80));
#endregion

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

#region — Міграції
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pending = await db.Database.GetPendingMigrationsAsync();
    if (pending.Any())
    {
        Console.WriteLine("Applying database migrations...");
        await db.Database.MigrateAsync();
        Console.WriteLine("Migrations complete.");
    }
}
#endregion


app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        logger.LogError(ex, "Unhandled exception occurred");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Something went wrong." });
    });
});



#region — HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
#endregion

app.Run();