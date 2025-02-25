using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using prjGroupB.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace prjGroupB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventRegistrationController : ControllerBase
    {
        private readonly dbGroupBContext _context;
        private readonly string _secretKey = "b6t8fJH2WjwYgJt7XPTqVX37WYgKs8TZ"; // JWT 密鑰 (與 AuthController 相同)

        public EventRegistrationController(dbGroupBContext context)
        {
            _context = context;
        }

        // 🔹 取得活動報名列表
        [HttpGet("{eventId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRegistrations(int eventId)
        {
            var registrations = await _context.TEventRegistrationForms
                .Where(r => r.FEventId == eventId)
                .Include(r => r.FUser)
                .Select(r => new
                {
                    r.FEventRegistrationFormId,
                    r.FEventId,
                    r.FUserId,
                    UserName = r.FUser.FUserName,
                    r.FEregistrationDate,
                    r.FRegistrationStatus
                })
                .ToListAsync();

            return Ok(registrations);
        }

        // 🔹 使用者報名活動（確保 JWT Cookie 驗證）
        [HttpPost("register")]
        public async Task<IActionResult> RegisterForEvent([FromBody] TEventRegistrationForm registration)
        {
            // ✅ 從 Cookie 取得 JWT Token
            var token = Request.Cookies["jwt_token"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "請先登入" });
            }

            // ✅ 解析 JWT Token 取得 User ID
            var userId = GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized(new { message = "無效的登入憑證" });
            }

            // ✅ 檢查活動是否存在
            var eventItem = await _context.TEvents.FindAsync(registration.FEventId);
            if (eventItem == null)
            {
                return NotFound(new { message = "活動不存在" });
            }

            // ✅ 檢查活動是否已達報名上限
            int registeredCount = await _context.TEventRegistrationForms
                .CountAsync(r => r.FEventId == registration.FEventId);

            if (eventItem.FMaxParticipants != null && registeredCount >= eventItem.FMaxParticipants)
            {
                return BadRequest(new { message = "報名人數已滿，無法報名" });
            }

            // ✅ 檢查是否已報名
            var existingRegistration = await _context.TEventRegistrationForms
                .FirstOrDefaultAsync(r => r.FEventId == registration.FEventId && r.FUserId == userId);

            if (existingRegistration != null)
            {
                return Conflict(new { message = "您已報名過此活動" });
            }

            // ✅ 設定報名資訊
            registration.FUserId = (int)userId;
            registration.FEregistrationDate = DateTime.UtcNow;
            registration.FRegistrationStatus = "已報名";

            _context.TEventRegistrationForms.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(new { message = "報名成功" });
        }

        // ✅ JWT 解碼方法
        private int? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = "http://localhost:7112",
                    ValidAudience = "http://localhost:4200",
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true,
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                return userIdClaim != null ? int.Parse(userIdClaim) : (int?)null;
            }
            catch
            {
                return null;
            }
        }
    }
}