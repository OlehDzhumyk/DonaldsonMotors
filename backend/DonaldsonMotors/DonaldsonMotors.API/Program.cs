using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DonaldsonMotors.API.Data;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Services;
using DonaldsonMotors.API.Options;
using RazorLight;


var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));


// --- Logging Configuration ---
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// --- Configuration Reading ---
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// --- Service Registration ---

builder.Services.AddControllers();

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Register Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
{
    opts.User.RequireUniqueEmail = true;
    opts.Password.RequiredLength = 6;
    opts.Password.RequireDigit = false;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Register Authentication & Authorization
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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});
builder.Services.AddAuthorization();

// Register Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Application Service Registration ---

// Register Unit of Work. This is the single point of access to all repositories.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// NOTE: We no longer need to register each repository individually.
// The UnitOfWork now manages their lifecycle, simplifying this section.

// Register Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<IServiceTypeService, ServiceTypeService>();

// Configure EmailSettings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));

// Register EmailService
builder.Services.AddScoped<IEmailService, EmailService>();

// RazorLight Engine registration
var razorLightEngine = new RazorLightEngineBuilder()
                        .UseFileSystemProject(AppContext.BaseDirectory)
                        .UseMemoryCachingProvider()
                        .Build();
builder.Services.AddSingleton<IRazorLightEngine>(razorLightEngine);


// Define a CORS policy
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000", // Port from your production docker setup
                                           "http://localhost:5173")  // Port from Vite dev server
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});


// =================================================================================
// --- Application Build and Middleware Pipeline ---
// =================================================================================

var app = builder.Build();

// Automatically apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
    {
        Console.WriteLine("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Migrations complete.");
    }

    // Seed initial data
    // This should run after migrations are applied
    try
    {
        await DbInitializer.SeedDataAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during DB seeding.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Can be enabled for production

// Use the CORS policy here. It must be called after UseRouting and before UseAuthorization.
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication(); // IMPORTANT: Must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
// Lets the integration tests start the app with WebApplicationFactory<Program>
public partial class Program;
