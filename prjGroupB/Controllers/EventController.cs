using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly dbGroupBContext _context;

    public EventController(dbGroupBContext context)
    {
        _context = context;
    }

    // ✅ **取得所有活動 (包含篩選)**
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetEvents()
    {
        try
        {
            var events = await _context.TEvents
                .Include(e => e.TEventLocations)
                .Include(e => e.TEventImages)
                .Select(e => new
                {
                    e.FEventId,
                    e.FEventName,
                    e.FEventDescription,
                    e.FEventStartDate,
                    e.FEventEndDate,
                    FLocation = e.TEventLocations.Any()
                        ? e.TEventLocations.FirstOrDefault().FLocationName
                        : "未知地點",
                    FParticipant = e.FCurrentParticipants, // ✅ 直接使用資料庫現有人數
                    FDuration = e.FEventDuration, // ✅ 直接使用活動天數
                    FPrice = e.FEventFee, // ✅ 直接使用活動價格
                    ImageBase64 = e.TEventImages.Any()
                        ? "data:image/png;base64," + Convert.ToBase64String(e.TEventImages.First().FEventImage)
                        : null
                })
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"後端發生錯誤: {ex.Message}");
        }
    }

    // ✅ **取得單一活動**
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetEvent(int id)
    {
        var eventItem = await _context.TEvents
            .Include(e => e.TEventLocations)
            .Include(e => e.TEventImages)
            .FirstOrDefaultAsync(e => e.FEventId == id);

        if (eventItem == null)
        {
            return NotFound();
        }

        var eventImage = eventItem.TEventImages.FirstOrDefault();
        string defaultImage = "https://your-cdn.com/default-event.jpg";

        var result = new
        {
            eventItem.FEventId,
            eventItem.FEventName,
            eventItem.FEventDescription,
            eventItem.FEventStartDate,
            eventItem.FEventEndDate,
            FLocation = eventItem.TEventLocations.Any()
                ? eventItem.TEventLocations.FirstOrDefault().FLocationName
                : "未提供",
            FParticipant = eventItem.FCurrentParticipants, // ✅ 正確取人數
            FDuration = eventItem.FEventDuration, // ✅ 正確取天數
            FPrice = eventItem.FEventFee, // ✅ 正確取價格
            ImageBase64 = eventImage != null
                ? "data:image/png;base64," + Convert.ToBase64String(eventImage.FEventImage)
                : defaultImage
        };

        return Ok(result);
    }
}