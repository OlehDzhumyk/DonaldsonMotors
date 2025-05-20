// Interfaces/Services/IAuthService.cs
using DonaldsonMotors.API.DTOs.Auth;
using System.Threading.Tasks;

namespace DonaldsonMotors.API.Interfaces.Services
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterCustomerAsync(RegisterRequestDto dto);
        Task<RegisterResponseDto> RegisterStaffAsync(RegisterRequestDto dto);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
    }
}
