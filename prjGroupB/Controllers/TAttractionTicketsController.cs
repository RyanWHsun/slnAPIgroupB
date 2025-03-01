using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TAttractionTicketsController : ControllerBase {
        private readonly dbGroupBContext _context;

        public TAttractionTicketsController(dbGroupBContext context) {
            _context = context;
        }

        // GET: api/TAttractionTickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TAttractionTicketDTO>>> GetTAttractionTickets() {
            try {
                var attractionTicketDTOs = await _context.TAttractionTickets
                    .Select(ticket => new TAttractionTicketDTO {
                        FAttractionTicketId = ticket.FAttractionTicketId,
                        FAttractionId = ticket.FAttractionId,
                        FAttractionName = ticket.FAttraction.FAttractionName,
                        FTicketType = ticket.FTicketType,
                        FPrice = ticket.FPrice,
                        FDiscountInformation = ticket.FDiscountInformation,
                        FCreatedDate = ticket.FCreatedDate
                    }).ToListAsync();

                return Ok(attractionTicketDTOs);
            }
            catch (DbException ex) {
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // GET: api/TAttractionTickets/Search?isDistinct=true&pageSize=9&pageIndex=0&orderBy=createdDate
        [HttpGet]
        [Route("Search")]
        public async Task<ActionResult<IEnumerable<TAttractionTicketDTO>>> GetTAttractionTickets(bool isDistinct, int pageSize = 9, int pageIndex = 0, string orderBy = "") {
            try {
                var tickets = new List<TAttractionTicket>();
                // 先載入所有資料到記憶體
                var allTickets = await _context.TAttractionTickets
                    .Include(t => t.FAttraction)
                    .ToListAsync();

                if (isDistinct) {
                    // 在記憶體中分組並選取每組的第一筆
                    tickets = allTickets
                        .GroupBy(ticket => ticket.FAttractionId) // 按 FAttractionId 分組
                        .Select(group => group.First()) // 選取每組的第一筆
                        .ToList();
                }
                else {
                    tickets = allTickets;
                }

                // 依照 createdDate 排序
                if (orderBy == "createdDate") {
                    // OrderByDescending() 會根據 FCreatedDate（可能是日期欄位）降序排列，即「最新的記錄」在最前。
                    tickets = tickets.OrderByDescending(ticket => ticket.FCreatedDate).ToList();
                }

                // .Skip(pageSize * pageIndex):
                // 跳過 pageSize *pageIndex 筆資料。
                // 假設 pageIndex = 0，則跳過 10 * 0 = 0 筆，表示從第一筆開始。
                // 假設 pageIndex = 1，則跳過 10 * 1 = 10 筆，表示從第 11 筆開始。

                // .Take(pageSize):
                // 取出最多 pageSize 筆資料。
                // 在這裡，表示從跳過的筆數後開始，取出最多 10 筆資料。
                tickets = tickets
                    .Skip(pageSize * pageIndex)
                    .Take(pageSize).ToList();

                // .Any() 是 LINQ 的一個方法，檢查集合中是否存在至少一個元素。
                // 如果集合中有資料，.Any() 會回傳 true。
                // 如果集合為空，.Any() 會回傳 false。
                if (tickets == null || !tickets.Any()) {
                    return Ok(new List<TAttractionTicketDTO>());
                }

                var ticketDTOs = tickets.Select(ticket => new TAttractionTicketDTO {
                    FAttractionTicketId = ticket.FAttractionTicketId,
                    FAttractionId = ticket.FAttractionId,
                    FAttractionName = ticket.FAttraction.FAttractionName,
                    FTicketType = ticket.FTicketType,
                    FPrice = ticket.FPrice,
                    FDiscountInformation = ticket.FDiscountInformation,
                    FCreatedDate = ticket.FCreatedDate
                }).ToList();

                return Ok(ticketDTOs);
            }
            catch (DbException ex) {
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // GET: api/TAttractionTickets/5
        // id is the attraction id
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<TAttractionTicketDTO>>> GetTAttractionTicket(int id) {
            try {
                var attractionTickets = await _context.TAttractionTickets
                    .Include(ticket => ticket.FAttraction)
                    .Where(ticket => ticket.FAttractionId == id)
                    .ToListAsync();

                // .Any() 是 LINQ 的一個方法，檢查集合中是否存在至少一個元素。
                // 如果集合中有資料，.Any() 會回傳 true。
                // 如果集合為空，.Any() 會回傳 false。
                if (attractionTickets == null || !attractionTickets.Any()) {
                    return Ok(new List<TAttractionTicketDTO>());
                }

                var ticketDTOs = attractionTickets.Select(attractionTicket => new TAttractionTicketDTO {
                    FAttractionTicketId = attractionTicket.FAttractionTicketId,
                    FAttractionId = attractionTicket.FAttractionId,
                    FAttractionName = attractionTicket.FAttraction.FAttractionName,
                    FTicketType = attractionTicket.FTicketType,
                    FPrice = attractionTicket.FPrice,
                    FDiscountInformation = attractionTicket.FDiscountInformation
                }).ToList();

                return Ok(ticketDTOs);
            }
            catch (DbException ex) {
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // 取得門票種類
        // GET: api/TAttractionTickets/{ticketId}/types
        // id is the attraction id
        [HttpGet("{attractionId}/types")]
        public async Task<ActionResult<List<string>>> GetTAttractionTicketTypeById(int attractionId) {
            try {
                List<TAttractionTicket> attractionTickets = await _context.TAttractionTickets
                    .Where(ticket => ticket.FAttractionId == attractionId)
                    .ToListAsync();

                if (attractionTickets == null || !attractionTickets.Any()) {
                    return Ok(new List<string>());
                }

                var ticketTypes = new List<string>();
                foreach (var ticket in attractionTickets) {
                    ticketTypes.Add(ticket.FTicketType);
                }
                return Ok(ticketTypes);
            }
            catch (DbException ex) {
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        [HttpGet("Count")]
        public async Task<ActionResult<int>> GetTicketQuantities() {
            try {
                var count = await _context.TAttractionTickets
                    .Select(t => t.FAttractionId) // 只取 fAttractionId
                    .Distinct()
                    .CountAsync();

                return Ok(count);
            }
            catch (DbException ex) {
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // PUT: api/TAttractionTickets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTAttractionTicket(int id, TAttractionTicketDTO attractionTicketDTO) {
            try {
                if (id != attractionTicketDTO.FAttractionTicketId) {
                    return BadRequest(new { error = "ticket Id 不符合" });
                }

                var attractionTicket = await _context.TAttractionTickets.FindAsync(id);
                if (attractionTicket == null) {
                    return NotFound(new { error = "找不到 ticket" });
                }

                attractionTicket.FAttractionTicketId = attractionTicketDTO.FAttractionTicketId;
                attractionTicket.FAttractionId = attractionTicketDTO.FAttractionId;
                attractionTicket.FTicketType = attractionTicketDTO.FTicketType;
                attractionTicket.FPrice = attractionTicketDTO.FPrice;
                attractionTicket.FDiscountInformation = attractionTicketDTO.FDiscountInformation;
                attractionTicket.FCreatedDate = attractionTicketDTO.FCreatedDate;

                _context.Entry(attractionTicket).State = EntityState.Modified;

                try {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!TAttractionTicketExists(id)) {
                        return NotFound(new { error = "ticket 不存在" });
                    }
                    else {
                        throw;
                    }
                }

                return NoContent();
            }
            catch (DbException ex) {
                return StatusCode(500, new { error = "資料庫連線錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // POST: api/TAttractionTickets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TAttractionTicketDTO>> PostTAttractionTicket(TAttractionTicketDTO attractionTicketDTO) {
            try {
                TAttractionTicket attractionTicket = new TAttractionTicket {
                    // Id 是資料庫自動產生的，這裡先預設為 0
                    FAttractionTicketId = 0,
                    FAttractionId = attractionTicketDTO.FAttractionId,
                    FTicketType = attractionTicketDTO.FTicketType,
                    FPrice = attractionTicketDTO.FPrice,
                    FDiscountInformation = attractionTicketDTO.FDiscountInformation,
                    FCreatedDate = DateTime.Now
                };

                _context.TAttractionTickets.Add(attractionTicket);

                // 1. 新的記錄插入資料庫。
                // 2. 資料庫生成並返回新的 FAttractionTicketId。
                // 3. EF 將新生成的 ID 更新到 attractionTicket.FAttractionTicketId。
                await _context.SaveChangesAsync();

                attractionTicketDTO.FAttractionTicketId = attractionTicket.FAttractionTicketId;
                // 更新 attractionTicketDTO 的 FAttractionTicketId
                // ASP.NET Core 提供的 HTTP 201 Created 回應 方法
                // 201 Created HTTP 狀態碼（表示資源已成功建立）。
                // Location 標頭，包含新建立的資源的 URL（透過 nameof(GetTAttractionTicket) 指定）。
                // 回傳 JSON 物件，這裡是 attractionTicketDTO，包含新建立的票券資訊。
                // CreatedAtAction 會自動根據 GetTAttractionTicket 方法的路由格式，生成返回的 URL，例如：GET /api/TAttractionTickets/123
                // new { id = attractionTicket.FAttractionId }
                // 這是路由參數，表示 GetTAttractionTicket(int id) 需要的 id 參數。
                // 假設：

                // FAttractionTicketId = 123
                // FAttractionId = 5
                // GetTAttractionTicket 的路由是 /api/TAttractionTickets/{id}
                // 則 API 會回傳：
                // HTTP/1.1 201 Created
                // Location: /api/TAttractionTickets/123
                // Content-Type: application/json
                // {
                //   "FAttractionTicketId": 123,
                //   "FAttractionId": 5,
                //   "FTicketType": "成人票",
                //   "FPrice": 500,
                //   "FDiscountInformation": "無",
                //   "FCreatedDate": "2025-03-01T12:00:00Z"
                // }
                return CreatedAtAction(nameof(GetTAttractionTicket), new { id = attractionTicket.FAttractionId }, attractionTicketDTO);
            }
            catch (DbUpdateException ex) {
                return StatusCode(500, new { error = "資料庫更新錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        // DELETE: api/TAttractionTickets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTAttractionTicket(int id) {
            try {
                var tAttractionTicket = await _context.TAttractionTickets.FindAsync(id);
                if (tAttractionTicket == null) {
                    return NotFound(new { error = "找不到要刪除的 ticket" });
                }

                _context.TAttractionTickets.Remove(tAttractionTicket);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex) {
                return StatusCode(500, new { error = "資料庫更新錯誤", details = ex.Message });
            }
            catch (Exception ex) {
                return StatusCode(500, new { error = "內部錯誤", details = ex.Message });
            }
        }

        private bool TAttractionTicketExists(int id) {
            return _context.TAttractionTickets.Any(e => e.FAttractionTicketId == id);
        }
    }
}
