﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
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

    // ✅ **取得所有活動 (包含天數、人數、報名費、圖片)**
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
        FLocation = e.TEventLocations.FirstOrDefault() != null
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

            Console.WriteLine($"📌 API 回傳資料: {System.Text.Json.JsonSerializer.Serialize(events)}");

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
                .ThenInclude(r => r.TEventPayments) // 取得報名費
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

            // ✅ 統一變數名稱，確保前端可以讀取
            fLocation = eventItem.TEventLocations.Any()
                ? eventItem.TEventLocations.Select(l => l.FLocationName).FirstOrDefault()
                : "未提供",

            FDuration = eventItem.TEventSchedules.Any()
                ? (int)((eventItem.TEventSchedules.Max(s => (DateTime?)s.FEndTime).GetValueOrDefault()
                        - eventItem.TEventSchedules.Min(s => (DateTime?)s.FStartTime).GetValueOrDefault())
                        .TotalDays + 1)
                : 1,

            FParticipant = eventItem.TEventRegistrationForms.Count(),

            RegistrationFee = eventItem.TEventRegistrationForms
                .SelectMany(r => r.TEventPayments)
                .Where(p => p.FPaymentStatus == "Paid")
                .Sum(p => p.FAmount),

            // ✅ 確保圖片回傳正確的 Base64 或 URL
            imageBase64 = eventImage != null
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
}