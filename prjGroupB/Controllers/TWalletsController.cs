using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TWalletsController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TWalletsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TWallets
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TWallet>>> GetTWallets()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //var userId = 3004;
            var walletRecords = await _context.TWallets.Where(w=>w.FUserId == userId).ToListAsync();
            if (walletRecords == null || walletRecords.Count == 0)
            {
                return NotFound("尚無紀錄");
            }
            return walletRecords;
        }


        private bool TWalletExists(int id)
        {
            return _context.TWallets.Any(e => e.FTradeId == id);
        }
    }
}
