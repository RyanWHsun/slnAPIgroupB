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

    // ✅ **取得所有活動**
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.TEvents
            .Select(e => new
            {
                e.FEventId,
                e.FEventName,
                e.FEventDescription,
                e.FEventStartDate,
                e.FEventEndDate
            })
            .ToListAsync();

        return Ok(events);
    }

    // ✅ **取得單一活動**
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var eventItem = await _context.TEvents
            .Where(e => e.FEventId == id)
            .Select(e => new
            {
                e.FEventId,
                e.FEventName,
                e.FEventDescription,
                e.FEventStartDate,
                e.FEventEndDate
            })
            .FirstOrDefaultAsync();

        if (eventItem == null)
            return NotFound(new { message = "活動不存在" });

        return Ok(eventItem);
    }

    // ✅ **新增活動**
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromForm] TEventCreateRequestDTO request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "請求內容為空" });
        }

        if (string.IsNullOrEmpty(request.Name))
        {
            return BadRequest(new { message = "活動名稱不可為空" });
        }

        if (request.StartDate == null || request.EndDate == null)
        {
            return BadRequest(new { message = "開始日期與結束日期不可為空" });
        }

        var newEvent = new TEvent
        {
            FEventName = request.Name,
            FEventDescription = request.Description,
            FEventStartDate = request.StartDate.Value,
            FEventEndDate = request.EndDate.Value
        };

        _context.TEvents.Add(newEvent);
        await _context.SaveChangesAsync();

        if (request.Image != null)
        {
            await SaveEventImage(newEvent.FEventId, request.Image);
        }

        return Ok(new { message = "活動新增成功", eventId = newEvent.FEventId });
    }

    // ✅ **修改活動（支援圖片上傳）**
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromForm] TEventUpdateRequestDTO request)
    {
        var eventItem = await _context.TEvents.FindAsync(id);
        if (eventItem == null)
            return NotFound(new { message = "活動不存在" });

        // 只更新提供的欄位
        if (!string.IsNullOrEmpty(request.Name))
            eventItem.FEventName = request.Name;
        if (!string.IsNullOrEmpty(request.Description))
            eventItem.FEventDescription = request.Description;
        if (request.StartDate.HasValue)
            eventItem.FEventStartDate = request.StartDate.Value;
        if (request.EndDate.HasValue)
            eventItem.FEventEndDate = request.EndDate.Value;

        await _context.SaveChangesAsync();

        // 如果有圖片，才更新圖片
        if (request.Image != null)
        {
            await SaveEventImage(id, request.Image);
        }

        return Ok(new { message = "活動更新成功" });
    }

    // ✅ **刪除活動**
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventItem = await _context.TEvents
            .Include(e => e.TEventImages)  // 確保一起刪除相關圖片
            .FirstOrDefaultAsync(e => e.FEventId == id);

        if (eventItem == null)
            return NotFound(new { message = "活動不存在，無法刪除" });

        try
        {
            // 先刪除關聯的圖片，避免外鍵錯誤
            _context.TEventImages.RemoveRange(eventItem.TEventImages);

            // 刪除活動
            _context.TEvents.Remove(eventItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "活動刪除成功" });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { message = "刪除活動時發生錯誤，可能是因為此活動被其他資料表引用", error = ex.Message });
        }
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

        await SaveEventImage(eventId, image);

        return Ok(new { message = "圖片上傳成功" });
    }

    // ✅ **封裝圖片儲存邏輯**
    private async Task SaveEventImage(int eventId, IFormFile image)
    {
        var eventItem = await _context.TEvents
            .Include(e => e.TEventImages)
            .FirstOrDefaultAsync(e => e.FEventId == eventId);

        if (eventItem == null)
            return;

        using var ms = new MemoryStream();
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
}