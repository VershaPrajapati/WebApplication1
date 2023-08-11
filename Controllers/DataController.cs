using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api")]
    public class DataController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public DataController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("data")]
        [Authorize]
        public async Task<IActionResult> StoreData(DataStoreRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse("INVALID_REQUEST", "Invalid request. Please provide all required fields."));
            }

            var userId = GetUserIdFromClaims();
            var existingData = _dbContext.Data.FirstOrDefault(d => d.UserId == userId && d.Key == request.Key);

            if (existingData != null)
            {
                return Conflict(new ErrorResponse("KEY_EXISTS", "The provided key already exists in the database."));
            }

            var newData = new Data
            {
                UserId = userId,
                Key = request.Key,
                Value = request.Value
            };

            _dbContext.Data.Add(newData);
            await _dbContext.SaveChangesAsync();

            return Ok(new SuccessResponse("Data stored successfully."));
        }

        [HttpGet("data/{key}")]
        [Authorize]
        public IActionResult RetrieveData(string key)
        {
            var userId = GetUserIdFromClaims();
            var data = _dbContext.Data.FirstOrDefault(d => d.UserId == userId && d.Key == key);

            if (data == null)
            {
                return NotFound(new ErrorResponse("KEY_NOT_FOUND", "The provided key does not exist."));
            }

            return Ok(new SuccessResponse("Data retrieved successfully.", new { Key = data.Key, Value = data.Value }));
        }

        [HttpPut("data/{key}")]
        [Authorize]
        public async Task<IActionResult> UpdateData(string key, DataUpdateRequest request)
        {
            var userId = GetUserIdFromClaims();
            var data = await _dbContext.Data.FirstOrDefaultAsync(d => d.UserId == userId && d.Key == key);

            if (data == null)
            {
                return NotFound(new ErrorResponse("KEY_NOT_FOUND", "The provided key does not exist."));
            }

            data.Value = request.Value;
            await _dbContext.SaveChangesAsync();

            return Ok(new SuccessResponse("Data updated successfully."));
        }

        [HttpDelete("data/{key}")]
        [Authorize]
        public async Task<IActionResult> DeleteData(string key)
        {
            var userId = GetUserIdFromClaims();
            var data = await _dbContext.Data.FirstOrDefaultAsync(d => d.UserId == userId && d.Key == key);

            if (data == null)
            {
                return NotFound(new ErrorResponse("KEY_NOT_FOUND", "The provided key does not exist."));
            }

            _dbContext.Data.Remove(data);
            await _dbContext.SaveChangesAsync();

            return Ok(new SuccessResponse("Data deleted successfully."));
        }

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
