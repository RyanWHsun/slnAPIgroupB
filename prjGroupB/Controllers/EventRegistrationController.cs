using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace prjGroupB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventRegistrationController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public EventRegistrationController(dbGroupBContext context)
        {
            _context = context;
        }

        // 🔹 取得某活動的所有報名資訊
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
                    UserName = r.FUser.FUserName,  // 使用者名稱
                    r.FEregistrationDate,
                    r.FRegistrationStatus
                })
                .ToListAsync();

            return Ok(registrations);
        }

        // 🔹 使用者報名活動（需要登入）
        [HttpPost]
        [Authorize] // ⬅ 這行確保 API 只能讓已登入使用者呼叫
        public async Task<IActionResult> RegisterForEvent([FromBody] TEventRegistrationForm registration)
        {
            // ✅ 取得當前登入使用者 ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { message = "請先登入" });
            }

            // ✅ 檢查活動是否存在
            var eventItem = await _context.TEvents.FindAsync(registration.FEventId);
            if (eventItem == null)
            {
                return NotFound(new { message = "活動不存在" });
            }

            //// 檢查活動是否已達到報名人數上限
            //int registeredCount = await _context.TEventRegistrationForms
            //    .CountAsync(r => r.FEventId == registration.FEventId);

            //if (eventItem.FMaxParticipants != null && registeredCount >= eventItem.FMaxParticipants)
            //{
            //    return BadRequest(new { message = "報名人數已滿，無法報名" });
            //}

            //// 報名成功後，更新已報名人數
            //eventItem.FCurrentParticipants = registeredCount + 1;
            //await _context.SaveChangesAsync();

            // ✅ 檢查是否已報名
            var existingRegistration = await _context.TEventRegistrationForms
                .FirstOrDefaultAsync(r => r.FEventId == registration.FEventId && r.FUserId == int.Parse(userId));

            if (existingRegistration != null)
            {
                return Conflict(new { message = "您已報名過此活動" });
            }

            // ✅ 設定報名資訊
            registration.FUserId = int.Parse(userId); // 設定登入使用者 ID
            registration.FEregistrationDate = DateTime.UtcNow;
            registration.FRegistrationStatus = "已報名";

            _context.TEventRegistrationForms.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(new { message = "報名成功" });
        }

        // 🔹 取消報名（需要登入）
        [HttpDelete("{registrationId}")]
        [Authorize] // ⬅ 這行確保 API 只能讓已登入使用者呼叫
        public async Task<IActionResult> CancelRegistration(int registrationId)
        {
            // ✅ 取得當前登入使用者 ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { message = "請先登入" });
            }

            var registration = await _context.TEventRegistrationForms.FindAsync(registrationId);
            if (registration == null)
            {
                return NotFound(new { message = "報名資料不存在" });
            }

            // ✅ 確保只能取消自己的報名
            if (registration.FUserId != int.Parse(userId))
            {
                return Forbid();
            }

            _context.TEventRegistrationForms.Remove(registration);
            await _context.SaveChangesAsync();

            return Ok(new { message = "取消報名成功" });
        }
    }
}