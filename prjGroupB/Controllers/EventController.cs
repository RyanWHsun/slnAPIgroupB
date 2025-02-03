using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.Models;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly dbGroupBContext _context;

    public EventController(dbGroupBContext context)
    {
        _context = context;
    }

    // 取得所有活動
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetEvents()
    {
        var events = await _context.TEvents
            .Include(e => e.TEventLocations) // 確保載入關聯資料
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
                Locations = e.TEventLocations.Select(l => new { l.FLocationName }) // ✅ 只回傳 `FLocationName`
            })
            .ToListAsync();

        return Ok(events);
    }

    // 取得單一活動
    [HttpGet("{id}")]
    public async Task<ActionResult<TEvent>> GetEvent(int id)
    {
        var eventItem = await _context.TEvents.FindAsync(id);
        if (eventItem == null)
        {
            return NotFound();
        }
        return eventItem;
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
}