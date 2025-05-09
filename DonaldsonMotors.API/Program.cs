using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Versioning;
using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Interfaces.Repositories;
using DonaldsonMotors.API.Repositories;
using DonaldsonMotors.API.Interfaces.Services;
using DonaldsonMotors.API.Services;
using DonaldsonMotors.API.Models;
using DonaldsonMotors.API.Interfaces;

var builder = WebApplication.CreateBuilder(args);

#region — Configuration from env/appsettings —--------------------------------------------------
// 1) Connection string
var defaultConn = builder.Configuration.GetConnectionString("Default");

// 2) JWT settings
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtExpiryString = builder.Configuration["Jwt:ExpiryMinutes"] ?? "60";
var jwtExpiryMin = int.Parse(jwtExpiryString);
#endregion

#region — Service Registrations —---------------------------------------------------------------
// **NEW**: Register controllers
builder.Services.AddControllers();

// 1. EF Core: PostgreSQL
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(defaultConn));

// 2. ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
{
    opts.User.RequireUniqueEmail = true;
    opts.Password.RequiredLength = 6;
    opts.Password.RequireDigit = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 3. JWT Authentication
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
            ValidAudience = jwtAudience,
            ClockSkew = TimeSpan.Zero
        };
    });

// 4. Authorization
builder.Services.AddAuthorization();

// 5. API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// 6. Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 7. Repository layer (DI)
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();

// 8. Service layer (DI)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookingService, BookingService>();
// …other service registrations…

// 9. Kestrel endpoints
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80);
    //options.ListenAnyIP(8081, listen => listen.UseHttps());
});
#endregion

var app = builder.Build();

#region — Apply EF Migrations at Startup —-------------------------------------------------------
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

#region — HTTP Request Pipeline —----------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();   // must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();
#endregion

app.Run();
