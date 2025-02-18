using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TAttractionViewCountsController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionViewCountsController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionViewCounts/GetAllViewCount
        [HttpGet("GetAllViewCount")]
        public async Task<IEnumerable<TAttractionViewDTO>> GetAllViewCount() {
            var viewCounts = await _context.TAttractionViews.ToListAsync();
            var viewCountDTOs = viewCounts.Select(
                viewCount => new TAttractionViewDTO {
                    FId = viewCount.FId,
                    FAttractionId = viewCount.FAttractionId,
                    FViewCount = viewCount.FViewCount
                }).ToList();

            return viewCountDTOs;
        }

        // POST: api/TAttractionViewCounts/IncreaseViewCount
        // id 是景點 ID
        [HttpPost("IncreaseViewCount")]
        // 為了從 JSON Body 取得 id，要使用 [FromBody] 來解析
        public async Task<ActionResult<int>> IncreaseViewCount([FromBody] TAttractionIdDTO IdObj) {
            int attractionViewCount = 0;

            // HttpContext.Connection: 表示目前請求的連線資訊（如 IP、Port）。
            // HttpContext.Connection.RemoteIpAddress: 取得請求者的 IP 位址（用戶端 IP），會回傳一個 IPAddress 物件。
            string? userIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // DateTime.UtcNow: 取得目前的 UTC（協調世界時間）
            // .AddHours(8): 增加 8 小時，轉換成台灣時間
            // .AddMinutes(-5): 減少 5 分鐘，計算出 10 分鐘前的時間
            DateTime timeLimit = DateTime.UtcNow.AddHours(8).AddMinutes(-5); // 限制 5 分鐘內不重複計數
            // DateTime timeLimit = DateTime.UtcNow.AddHours(8).AddDays(-1); // 限制 1 天內不重複計數

            // 檢查是否在 10 分鐘內有觀看記錄
            var recentView = await _context.TAttractionViewLogs.FirstOrDefaultAsync(v => v.FAttractionId == IdObj.Id && v.FUserIp == userIp && v.FViewTime > timeLimit);

            if (recentView == null) {
                DateTimeOffset taiwanTimeOffset = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8));
                DateTime taiwanDateTime = taiwanTimeOffset.DateTime;

                // TAttractionViewLogs 插入新觀看記錄
                _context.TAttractionViewLogs.Add(new TAttractionViewLog { FAttractionId = IdObj.Id, FUserIp = userIp, FViewTime = taiwanDateTime });

                // 更新 TAttractionViews 表的 ViewCount
                var attraction = await _context.TAttractionViews.FirstOrDefaultAsync(v => v.FAttractionId == IdObj.Id);
                if (attraction != null) {
                    attraction.FViewCount++;
                    // ??（Null Coalescing Operator，空合併運算子）
                    // 如果 attraction.FViewCount 不是 null，則將其值賦予 attractionViewCount。
                    // 如果 attraction.FViewCount 是 null，則使用 0 作為預設值。
                    attractionViewCount = attraction.FViewCount ?? 0;
                }
                else {
                    _context.TAttractionViews.Add(new TAttractionView { FAttractionId = IdObj.Id, FViewCount = 1 });
                    attractionViewCount = 1;
                }
                await _context.SaveChangesAsync();
            }

            return Ok(attractionViewCount);
        }

    }
}
