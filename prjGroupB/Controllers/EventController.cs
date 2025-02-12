using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly dbGroupBContext _context;
    private readonly IImageService _imageService; // ✅ 確保 `_imageService` 存在

    public EventController(dbGroupBContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
    }

    // ✅ **取得所有活動**
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetEvents()
    {
        try
        {
            var events = await _context.TEvents
                .Include(e => e.TEventLocations)  // 確保載入地點資料
                .Include(e => e.TEventImages) // 載入圖片
                .Select(e => new
                {
                    e.FEventId,
                    e.FEventName,
                    e.FEventDescription,
                    e.FEventStartDate,
                    e.FEventEndDate,
                    Location = e.TEventLocations.Any()
                        ? e.TEventLocations.Select(l => l.FLocationName).FirstOrDefault()
                        : "未知地點", // 確保地點不為 null
                    ImageBase64 = e.TEventImages.Any()
                        ? "data:image/png;base64," + Convert.ToBase64String(e.TEventImages.First().FEventImage)
                        : null // 無圖片則回傳 null
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
            .Include(e => e.TEventImages) // 確保載入圖片
            .FirstOrDefaultAsync(e => e.FEventId == id);

        if (eventItem == null)
        {
            return NotFound();
        }

        var eventImage = eventItem.TEventImages.FirstOrDefault();

        var result = new
        {
            eventItem.FEventId,
            eventItem.FEventName,
            eventItem.FEventDescription,
            eventItem.FEventStartDate,
            eventItem.FEventEndDate,
            ImageBase64 = eventImage != null
                ? "data:image/png;base64," + Convert.ToBase64String(eventImage.FEventImage)
                : null
        };

        return Ok(result);
    }

    // ✅ **新增活動**
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromForm] TEventDTO eventDto, IFormFile image)
    {
        if (eventDto == null || string.IsNullOrEmpty(eventDto.Name))
        {
            return BadRequest(new { message = "活動資訊不完整" });
        }

        var newEvent = new TEvent
        {
            FEventName = eventDto.Name,
            FEventDescription = eventDto.Description,
            FEventStartDate = eventDto.StartDate,
            FEventEndDate = eventDto.EndDate
        };

        _context.TEvents.Add(newEvent);
        await _context.SaveChangesAsync(); // 先存活動

        // 處理圖片
        if (image != null)
        {
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);

            var eventImage = new TEventImage
            {
                FEventId = newEvent.FEventId,
                FEventImage = memoryStream.ToArray(),
                FImageType = image.ContentType
            };

            _context.TEventImages.Add(eventImage);
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "活動新增成功", eventId = newEvent.FEventId });
    }

    // ✅ **更新活動**
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromForm] TEventDTO eventDto, IFormFile image)
    {
        var existingEvent = await _context.TEvents.FindAsync(id);
        if (existingEvent == null)
        {
            return NotFound("找不到該活動");
        }

        existingEvent.FEventName = eventDto.Name;
        existingEvent.FEventDescription = eventDto.Description;
        existingEvent.FEventStartDate = eventDto.StartDate;
        existingEvent.FEventEndDate = eventDto.EndDate;

        if (image != null)
        {
            var imageData = await _imageService.SaveImage(image); // 取得 byte[]
            var existingImage = await _context.TEventImages.FirstOrDefaultAsync(img => img.FEventId == id);

            if (existingImage != null)
            {
                existingImage.FEventImage = imageData;
                existingImage.FImageType = image.ContentType;
                _context.TEventImages.Update(existingImage);
            }
            else
            {
                var newImage = new TEventImage
                {
                    FEventId = id,
                    FEventImage = imageData,
                    FImageType = image.ContentType
                };
                _context.TEventImages.Add(newImage);
            }

            await _context.SaveChangesAsync();
        }

        _context.Entry(existingEvent).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(existingEvent);
    }

    // ✅ **刪除活動**
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventItem = await _context.TEvents.FindAsync(id);
        if (eventItem == null)
        {
            return NotFound();
        }

        // **刪除對應圖片**
        var images = _context.TEventImages.Where(img => img.FEventId == id);
        _context.TEventImages.RemoveRange(images);

        _context.TEvents.Remove(eventItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}


