using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DonaldsonMotors.API.DTOs.Auth;
using DonaldsonMotors.API.Interfaces;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Exceptions;

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
        /// <param name="dto">The registration details for the new customer.</param>
        /// <returns>An authentication response with a JWT token.</returns>
        /// <response code="200">Returns the authentication response upon successful registration.</response>
        /// <response code="400">If the request payload is invalid.</response>
        /// <response code="409">If a user with the same email already exists.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterRequestDto dto)
        {
            // Enforce the role is 'Customer' for this public endpoint.
            dto.Role = Roles.Customer;
            _logger.LogInformation("Attempting to register new customer: {Email}", dto.Email);

            var response = await _authService.RegisterAsync(dto);
            _logger.LogInformation("Customer registered successfully: {Email}", dto.Email);
            return Ok(response);
        }

        /// <summary>
        /// Registers a new staff member (e.g., Mechanic, Manager). Accessible only by Managers.
        /// </summary>
        /// <param name="dto">The registration details for the new staff member.</param>
        /// <returns>An authentication response with a JWT token.</returns>
        /// <response code="200">Returns the authentication response upon successful registration.</response>
        /// <response code="400">If the request payload or role is invalid.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="403">If the authenticated user is not a Manager.</response>
        /// <response code="409">If a user with the same email already exists.</response>
        [HttpPost("register/staff")]
        [Authorize(Roles = Roles.Manager)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterRequestDto dto)
        {
            _logger.LogInformation("Manager attempting to register new staff: {Email}, Role: {Role}", dto.Email, dto.Role);
            var response = await _authService.RegisterAsync(dto);
            _logger.LogInformation("Staff registered successfully: {Email}", dto.Email);
            return Ok(response);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="dto">The user's login credentials.</param>
        /// <returns>An authentication response with a JWT token.</returns>
        /// <response code="200">Returns the authentication response on successful login.</response>
        /// <response code="401">If the credentials are invalid.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            _logger.LogInformation("Login attempt for user: {Email}", dto.Email);
            var response = await _authService.LoginAsync(dto);
            _logger.LogInformation("User {Email} logged in successfully.", dto.Email);
            return Ok(response);
        }
    }
}