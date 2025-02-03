using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.Models;
using System;
using System.Linq;
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

        // 取得某活動的所有報名資訊
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

        // 使用者報名活動
        [HttpPost]
        public async Task<IActionResult> RegisterForEvent([FromBody] TEventRegistrationForm registration)
        {
            // 檢查活動是否存在
            var eventItem = await _context.TEvents.FindAsync(registration.FEventId);
            if (eventItem == null)
            {
                return NotFound(new { message = "活動不存在" });
            }

            // 確保使用者 ID 存在
            if (registration.FUserId == null)
            {
                return BadRequest(new { message = "請提供使用者 ID" });
            }

            // 檢查是否已報名
            var existingRegistration = await _context.TEventRegistrationForms
                .FirstOrDefaultAsync(r => r.FEventId == registration.FEventId && r.FUserId == registration.FUserId);

            if (existingRegistration != null)
            {
                return Conflict(new { message = "您已經報名過此活動" });
            }

            // 設定報名時間
            registration.FEregistrationDate = DateTime.UtcNow;
            registration.FRegistrationStatus = "已報名";

            _context.TEventRegistrationForms.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(new { message = "報名成功" });
        }

        // 取消報名
        [HttpDelete("{registrationId}")]
        public async Task<IActionResult> CancelRegistration(int registrationId)
        {
            var registration = await _context.TEventRegistrationForms.FindAsync(registrationId);
            if (registration == null)
            {
                return NotFound(new { message = "報名資料不存在" });
            }

            _context.TEventRegistrationForms.Remove(registration);
            await _context.SaveChangesAsync();

            return Ok(new { message = "取消報名成功" });
        }
    }
}