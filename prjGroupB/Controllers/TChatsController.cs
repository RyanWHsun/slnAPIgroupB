﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NuGet.Protocol.Plugins;
using prjGroupB.DTO;
using prjGroupB.Hubs;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TChatsController : ControllerBase
    {
        private readonly dbGroupBContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public TChatsController(dbGroupBContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
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
                    FSentAt = e.FSentAt.ToString()
                });
        }
        [HttpGet("Contact")]
        [Authorize]
        public IEnumerable<int?> GetContact()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return _context.TChats
                .Where(c => c.FSenderId == userId || c.FReceiverId == userId)
                .GroupBy(c => c.FSenderId == userId ? c.FReceiverId : c.FSenderId)
                .Select(g => new
                {
                    ContactedUserID = g.Key,
                    LastContactTime = g.Max(c => c.FSentAt)
                })
                .OrderByDescending(g => g.LastContactTime)
                .Select(e => e.ContactedUserID);
        }


        // POST: api/TChats
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostTChat(TChatsDTO ChatsDTO)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (userId == ChatsDTO.FReceiverId)
                return Unauthorized(new { message = "你沒有權限留言" });
            TChat chat = new TChat
            {
                FSenderId = userId,
                FReceiverId = ChatsDTO.FReceiverId,
                FMessageText = ChatsDTO.FMessageText,
                FSentAt = DateTime.Now
            };
            _context.TChats.Add(chat);
            await _context.SaveChangesAsync();
            ChatsDTO.FChatId=chat.FChatId;
            ChatsDTO.FSenderId = chat.FSenderId;
            ChatsDTO.FSentAt = chat.FSentAt.ToString();
            await _hubContext.Clients.Users(ChatsDTO.FSenderId.ToString(),ChatsDTO.FReceiverId.ToString()).SendAsync("ReceivePrivateMessage", ChatsDTO);
            return Ok(new { message="新增留言成功"});
        }
    }
}
