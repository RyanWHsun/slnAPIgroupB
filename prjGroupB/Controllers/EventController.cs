using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly dbGroupBContext _context;

    public EventController(dbGroupBContext context)
    {
        _context = context;
    }

    // 取得所有活動，包含地點、人數、圖片
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetEvents()
    {
        try
        {
            var events = await _context.TEvents
                .Include(e => e.TEventLocations)
                .Include(e => e.TEventRegistrationForms)
                .Select(e => new
                {
                    e.FEventId,
                    e.FEventName,
                    e.FEventDescription,
                    e.FEventStartDate,
                    e.FEventEndDate,
                    e.FEventCreatedDate,
                    e.FEventUpdatedDate,
                    e.FEventUrl,
                    e.FEventIsActive,
                    Location = e.TEventLocations.Any() ? e.TEventLocations.Select(l => l.FLocationName).FirstOrDefault() : "未知地點",
                    ParticipantCount = e.TEventRegistrationForms != null ? e.TEventRegistrationForms.Count() : 0
                })
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[❌] API 發生錯誤: {ex.Message}");
            Console.WriteLine($"[🔍] StackTrace: {ex.StackTrace}");
            return StatusCode(500, $"後端發生錯誤: {ex.Message}");
        }
    }

    // 取得單一活動
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetEvent(int id)
    {
        try
        {
            var eventItem = await _context.TEvents
                .Include(e => e.TEventLocations)
                .Include(e => e.TEventImages)
                .Include(e => e.TEventRegistrationForms)
                .Where(e => e.FEventId == id)
                .FirstOrDefaultAsync();

            if (eventItem == null)
            {
                Console.WriteLine($"[⚠️] EventId {id} 找不到對應活動");
                return NotFound();
            }

            // 檢查哪些集合可能為 null
            Console.WriteLine($"[🔍] 取得活動: {eventItem.FEventName}");
            Console.WriteLine($"[🔍] 地點數量: {(eventItem.TEventLocations != null ? eventItem.TEventLocations.Count : 0)}");
            Console.WriteLine($"[🔍] 圖片數量: {(eventItem.TEventImages != null ? eventItem.TEventImages.Count : 0)}");
            Console.WriteLine($"[🔍] 參與者數量: {(eventItem.TEventRegistrationForms != null ? eventItem.TEventRegistrationForms.Count : 0)}");

            // 確保不會發生 null 例外
            var location = eventItem.TEventLocations?.FirstOrDefault()?.FLocationName ?? "未知地點";
            var imageUrl = eventItem.TEventImages?.FirstOrDefault()?.FEventImage != null
                ? "data:image/png;base64," + Convert.ToBase64String(eventItem.TEventImages.First().FEventImage)
                : null;
            var participantCount = eventItem.TEventRegistrationForms?.Count() ?? 0;

            var result = new
            {
                eventItem.FEventId,
                eventItem.FEventName,
                eventItem.FEventDescription,
                eventItem.FEventStartDate,
                eventItem.FEventEndDate,
                eventItem.FEventCreatedDate,
                eventItem.FEventUpdatedDate,
                eventItem.FEventUrl,
                eventItem.FEventIsActive,
                Location = location,
                ImageUrl = imageUrl,
                ParticipantCount = participantCount
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[❌] API 發生錯誤: {ex.Message}");
            Console.WriteLine($"[🔍] StackTrace: {ex.StackTrace}");
            return StatusCode(500, $"後端發生錯誤: {ex.Message}");
        }
    }

    // 新增活動
    [HttpPost]
    public async Task<ActionResult<TEvent>> CreateEvent(TEvent eventItem)
    {
        _context.TEvents.Add(eventItem);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEvent), new { id = eventItem.FEventId }, eventItem);
    }

    // 更新活動
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, TEvent eventItem)
    {
        if (id != eventItem.FEventId)
        {
            return BadRequest();
        }

        _context.Entry(eventItem).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.TEvents.Any(e => e.FEventId == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // 刪除活動
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventItem = await _context.TEvents.FindAsync(id);
        if (eventItem == null)
        {
            return NotFound();
        }

        _context.TEvents.Remove(eventItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // 收藏/取消收藏活動
    [HttpPost("favorite/{eventId}/{userId}")]
    public async Task<IActionResult> FavoriteEvent(int eventId, int userId)
    {
        var favorite = await _context.TEventFavorites
            .FirstOrDefaultAsync(f => f.FEventId == eventId && f.FUserId == userId);

        if (favorite != null)
        {
            _context.TEventFavorites.Remove(favorite); // 取消收藏
        }
        else
        {
            _context.TEventFavorites.Add(new TEventFavorite
            {
                FEventId = eventId,
                FUserId = userId,
                FCreatedDate = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = favorite == null ? "收藏成功" : "取消收藏" });
    }

    // 取得用戶收藏的活動
    [HttpGet("favorites/{userId}")]
    public async Task<ActionResult<IEnumerable<int>>> GetUserFavorites(int userId)
    {
        var favorites = await _context.TEventFavorites
            .Where(f => f.FUserId == userId)
            .Select(f => f.FEventId)
            .ToListAsync();

        return Ok(favorites);
    }
}
