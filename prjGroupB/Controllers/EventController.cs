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
                .Include(e => e.TEventSchedules)
                .Include(e => e.TEventRegistrationForms)
                    .ThenInclude(r => r.TEventPayments)
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
                    FParticipant = e.TEventRegistrationForms
                        .Count(r => r.FRegistrationStatus == "Confirmed"),
                    RegistrationFee = e.TEventRegistrationForms
                        .SelectMany(r => r.TEventPayments)
                        .Where(p => p.FPaymentStatus == "Paid")
                        .Sum(p => p.FAmount),
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
            .Include(e => e.TEventSchedules)
            .Include(e => e.TEventRegistrationForms)
                .ThenInclude(r => r.TEventPayments)
            .FirstOrDefaultAsync(e => e.FEventId == id);

        if (eventItem == null)
        {
            return NotFound();
        }

        var eventImage = eventItem.TEventImages.FirstOrDefault();
        string defaultImage = "https://your-cdn.com/default-event.jpg"; // ✅ 預設圖片 URL

        var result = new
        {
            eventItem.FEventId,
            eventItem.FEventName,
            eventItem.FEventDescription,
            eventItem.FEventStartDate,
            eventItem.FEventEndDate,
            fLocation = eventItem.TEventLocations.Any()
                ? eventItem.TEventLocations.FirstOrDefault().FLocationName
                : "未提供",
            FParticipant = eventItem.TEventRegistrationForms.Count(),
            RegistrationFee = eventItem.TEventRegistrationForms
                .SelectMany(r => r.TEventPayments)
                .Where(p => p.FPaymentStatus == "Paid")
                .Sum(p => p.FAmount),
            imageBase64 = eventImage != null
                ? "data:image/png;base64," + Convert.ToBase64String(eventImage.FEventImage)
                : defaultImage
        };

        return Ok(result);
    }
}