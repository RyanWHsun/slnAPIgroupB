using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TChatsController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TChatsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TChats/5
        // id 跟誰的聊天室
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IEnumerable<TChatsDTO>> GetTChat(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (userId == id)
                return null;
            return _context.TChats
                .Where(c => (c.FSenderId == userId && c.FReceiverId == id)
                || (c.FSenderId == id && c.FReceiverId == userId))
                .Select(e => new TChatsDTO
                {
                    FChatId = e.FChatId,
                    FSenderId = e.FSenderId,
                    FReceiverId = e.FReceiverId,
                    FMessageText = e.FMessageText,
                    FSentAt = e.FSentAt
                });
        }


        // POST: api/TChats
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<TChatsDTO> PostTChat(TChatsDTO ChatsDTO)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (userId == ChatsDTO.FReceiverId)
                return null;
            TChat chat = new TChat
            {
                FSenderId = userId,
                FReceiverId = ChatsDTO.FReceiverId,
                FMessageText = ChatsDTO.FMessageText,
                FSentAt = DateTime.Now
            };
            _context.TChats.Add(chat);
            await _context.SaveChangesAsync();
            ChatsDTO.FChatId = chat.FChatId;
            ChatsDTO.FSenderId = chat.FSenderId;
            return ChatsDTO;
        }
    }
}
