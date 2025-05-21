using DonaldsonMotors.API.Data.Entities;

public interface IJwtService
{
    Task<(string token, DateTime expiresAt)> GenerateTokenAsync(ApplicationUser user);
}
