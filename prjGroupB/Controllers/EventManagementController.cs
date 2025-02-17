using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class EventManagementController : ControllerBase
{
    private readonly dbGroupBContext _context;

    public EventManagementController(dbGroupBContext context)
    {
        _context = context;
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
        await _context.SaveChangesAsync();

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

    // ✅ **修改活動**
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] TEventDTO eventDto)
    {
        var eventItem = await _context.TEvents.FindAsync(id);
        if (eventItem == null)
            return NotFound(new { message = "活動不存在" });

        eventItem.FEventName = eventDto.Name;
        eventItem.FEventDescription = eventDto.Description;
        eventItem.FEventStartDate = eventDto.StartDate;
        eventItem.FEventEndDate = eventDto.EndDate;

        await _context.SaveChangesAsync();
        return Ok(new { message = "活動更新成功" });
    }

    // ✅ **刪除活動**
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventItem = await _context.TEvents.FindAsync(id);
        if (eventItem == null)
            return NotFound(new { message = "活動不存在" });

        _context.TEvents.Remove(eventItem);
        await _context.SaveChangesAsync();

        return Ok(new { message = "活動刪除成功" });
    }

    // ✅ **上傳活動圖片**
    [HttpPost("UploadEventImage/{eventId}")]
    public async Task<IActionResult> UploadEventImage(int eventId, IFormFile image)
    {
        if (image == null || !image.ContentType.StartsWith("image/"))
        {
            return BadRequest("請提供有效的圖片 (僅支援 JPG/PNG)");
        }

        var eventItem = await _context.TEvents
            .Include(e => e.TEventImages)
            .FirstOrDefaultAsync(e => e.FEventId == eventId);

        if (eventItem == null)
        {
            return NotFound("找不到該活動");
        }

        using (var ms = new MemoryStream())
        {
            await image.CopyToAsync(ms);
            var imageBytes = ms.ToArray();

            var existingImage = eventItem.TEventImages.FirstOrDefault();
            if (existingImage != null)
            {
                existingImage.FEventImage = imageBytes;
                existingImage.FImageType = image.ContentType;
            }
            else
            {
                eventItem.TEventImages.Add(new TEventImage
                {
                    FEventImage = imageBytes,
                    FEventId = eventId,
                    FImageType = image.ContentType
                });
            }

            await _context.SaveChangesAsync();
        }

        var base64String = "data:" + image.ContentType + ";base64," + Convert.ToBase64String(eventItem.TEventImages.FirstOrDefault().FEventImage);
        return Ok(new { imageUrl = base64String });
    }
}