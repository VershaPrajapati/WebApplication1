using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly string _jwtSecretKey = "your_secret_key"; // Change this to your actual secret key

        public AuthController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse("INVALID_REQUEST", "Invalid request. Please provide all required fields."));
            }

            var existingUser = _dbContext.Users.FirstOrDefault(u => u.Username == request.Username || u.Email == request.Email);
            if (existingUser != null)
            {
                return Conflict(new ErrorResponse("USER_EXISTS", "A user with the provided username or email already exists."));
            }

            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password, // In a real scenario, you should hash and store passwords securely
                FullName = request.FullName,
                Age = request.Age,
                Gender = request.Gender
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            var responseData = new
            {
                UserId = newUser.UserId,
                newUser.Username,
                newUser.Email,
                newUser.FullName,
                newUser.Age,
                newUser.Gender
            };

            return Ok(new SuccessResponse("User successfully registered!", responseData));
        }

        [HttpPost("token")]
        public IActionResult GenerateToken(UserLoginRequest request)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized(new ErrorResponse("INVALID_CREDENTIALS", "Invalid username or password."));
            }

            var token = GenerateAccessToken(user.UserId);
            return Ok(new SuccessResponse("Access token generated successfully.", new { Access_Token = token }));
        }

        private string GenerateAccessToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("userId", userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
