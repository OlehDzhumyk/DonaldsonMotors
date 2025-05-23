using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DonaldsonMotors.API.DTOs.Auth;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Data.Entities;

namespace DonaldsonMotors.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new customer. This endpoint is publicly accessible.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterRequestDto dto)
        {
            // Ensure the role is set to Customer for this endpoint
            dto.Role = Roles.Customer;

            _logger.LogInformation("Attempting to register new customer: {Email}", dto.Email);
            try
            {
                var response = await _authService.RegisterAsync(dto);
                _logger.LogInformation("Customer registered successfully: {Email}", dto.Email);
                return Ok(response);
            }
            catch (InvalidOperationException ex) // Catch specific exception for existing user
            {
                _logger.LogWarning("Registration failed for {Email}: {Error}", dto.Email, ex.Message);
                return Conflict(ex.Message); // Return 409 Conflict
            }
            catch (ArgumentException ex) // Catch validation errors from the service
            {
                _logger.LogWarning("Registration failed due to invalid arguments for {Email}: {Error}", dto.Email, ex.Message);
                return BadRequest(ex.Message); // Return 400 Bad Request
            }
        }

        /// <summary>
        /// Registers a new staff member. Accessible only by Managers.
        /// </summary>
        [HttpPost("register/staff")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterRequestDto dto)
        {
            _logger.LogInformation("Manager attempting to register new staff: {Email}, Role: {Role}", dto.Email, dto.Role);
            try
            {
                var response = await _authService.RegisterAsync(dto);
                _logger.LogInformation("Staff registered successfully: {Email}", dto.Email);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Staff registration failed for {Email}: {Error}", dto.Email, ex.Message);
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Staff registration failed due to invalid arguments for {Email}: {Error}", dto.Email, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            _logger.LogInformation("Login attempt for user: {Email}", dto.Email);
            try
            {
                var response = await _authService.LoginAsync(dto);
                _logger.LogInformation("User {Email} logged in successfully.", dto.Email);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Failed login attempt for {Email}: {Error}", dto.Email, ex.Message);
                return Unauthorized(ex.Message); // Return 401 Unauthorized
            }
        }
    }
}