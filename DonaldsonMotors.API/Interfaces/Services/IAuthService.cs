using DonaldsonMotors.API.Models;
using System.Threading.Tasks;

namespace DonaldsonMotors.API.Interfaces.Services
{
    /// <summary>
    /// Handles user registration and login (JWT issuance).
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user (Customer or Employee) and returns the created user + token.
        /// </summary>
        Task<AuthResult> RegisterAsync(RegisterModel model);

        /// <summary>
        /// Validates credentials and returns an auth token if successful.
        /// </summary>
        Task<AuthResult> LoginAsync(LoginModel model);
    }
}
