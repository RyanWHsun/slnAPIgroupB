using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TUsersController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TUsersController(dbGroupBContext context)
        {
            _context = context;
        }



        //查詢全部
        // GET: api/TUsers
        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<TUserDTO>> GetTUsers()
        {
            return _context.TUsers.Select(
                emp => new TUserDTO
                {
                    FUserId=emp.FUserId,
                    FUserName=emp.FUserName,
                    FUserRankId= (int)emp.FUserRankId,
                    FUserNickName=emp.FUserNickName,
                    FUserEmail=emp.FUserEmail,
                    FUserBirthday=emp.FUserBirthday,
                    FUserPhone=emp.FUserPhone,
                    FUserSex=emp.FUserSex,
                    FUserAddress=emp.FUserAddress,
                    FUserImage = emp.FUserImage != null ? Convert.ToBase64String(emp.FUserImage) : null,
                    FUserComeDate= (DateTime)emp.FUserComeDate
                    //FUserPassword=emp.FUserPassword
                }
                );
        }
        //查詢
        // GET: api/TUsers/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TUserDTO>> GetTUser(int id)
        {

            //尋找登入者ID
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 查詢指定 ID 的用戶
            var tUser = await _context.TUsers
                .Where(p => p.FUserId == userId).FirstOrDefaultAsync();

            // 若找不到用戶，返回 404
            if (tUser == null)
            {
                return NotFound(new { message = "用戶不存在或無權限存取" });
            }

            var userDTO = new TUserDTO
            {
                    FUserId = tUser.FUserId,
                    FUserName = tUser.FUserName,
                    FUserRankId = (int)tUser.FUserRankId,
                    FUserNickName = tUser.FUserNickName,
                    FUserEmail = tUser.FUserEmail,
                    FUserBirthday = tUser.FUserBirthday,
                    FUserPhone = tUser.FUserPhone,
                    FUserSex = tUser.FUserSex,
                    FUserAddress = tUser.FUserAddress,
                    FUserImage = tUser.FUserImage != null ? Convert.ToBase64String(tUser.FUserImage) : null,// 將 FUserImage 轉為 Base64 字串
                    FUserComeDate = (DateTime)tUser.FUserComeDate
                    //FUserPassword = tUser.FUserPassword
                };
            return userDTO;
        }


        //修改
        // PUT: api/TUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutTUser( [FromBody]TUserDTO userDTO)
        {
            //尋找登入者ID
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 查詢指定 ID 的用戶
            var tUser = await _context.TUsers
                .Where(p => p.FUserId == userId).FirstOrDefaultAsync();

            // 若找不到用戶，返回 404
            if (tUser == null)
            {
                return NotFound(new { message = "用戶不存在或無權限存取" });
            }


            // 查找要更新的用戶
            //TUser user = await _context.TUsers.FindAsync(id);

            //if (user == null)
            //{
            //    return BadRequest(new { message = "修改失敗，找不到用戶" });
            //}

            // 轉換 FUserImage（Base64 -> byte[]）
            if (!string.IsNullOrEmpty(userDTO.FUserImage))
            {
                tUser.FUserImage = Convert.FromBase64String(userDTO.FUserImage);
            }
            tUser.FUserName = userDTO.FUserName;
            tUser.FUserRankId = userDTO.FUserRankId;
            tUser.FUserNickName = userDTO.FUserNickName;
            tUser.FUserBirthday = userDTO.FUserBirthday;
            tUser.FUserPhone = userDTO.FUserPhone;
            tUser.FUserSex = userDTO.FUserSex;
            tUser.FUserAddress = userDTO.FUserAddress;

            // 如果提供新密碼，則進行雜湊
            if (!string.IsNullOrEmpty(userDTO.FUserPassword))
            {
                tUser.FUserPassword = HashPassword(userDTO.FUserPassword);
            }

            //設定為已修改狀態
            _context.Entry(tUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "修改失敗" });
            }
            return Ok(new { message = "修改成功" });
        }


        //新增
        // POST: api/TUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostTUser([FromBody] TUserDTO userDTO)
        {
            //擋住已註冊的Email
            bool haveEmail=await _context.TUsers.AnyAsync(u=>u.FUserEmail==userDTO.FUserEmail);
            if (haveEmail) {
                return BadRequest(new { message = "該Email已被使用" });
            }


            // 轉換 Base64 字串為 byte[]，確保圖片能夠正確存入資料庫
            byte[] userImage = string.IsNullOrEmpty(userDTO.FUserImage)
                ? null : Convert.FromBase64String(userDTO.FUserImage);


            TUser user = new TUser
            {
                FUserName = userDTO.FUserName,
                FUserRankId = userDTO.FUserRankId,
                FUserNickName = userDTO.FUserNickName,
                FUserEmail = userDTO.FUserEmail,
                FUserBirthday = userDTO.FUserBirthday,
                FUserPhone = userDTO.FUserPhone,
                FUserSex = userDTO.FUserSex,
                FUserAddress = userDTO.FUserAddress,
                FUserImage = userImage,
                FUserComeDate = DateTime.Now,
                FUserPassword = HashPassword(userDTO.FUserPassword)
            };

            // 嘗試寫入資料庫
            _context.TUsers.Add(user);
                await _context.SaveChangesAsync();
            return Ok(new { message = "註冊成功，會員編號:", userId = user.FUserId });
            }


        //密碼雜湊
        private string HashPassword(string password)
        {
            //BCrypt密碼雜湊
            return BCrypt.Net.BCrypt.HashPassword(password);
            //SHA-256
            //using (SHA256 sha256 = SHA256.Create())
            //{
            //    byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            //    return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            //}
        }

        // DELETE: api/TUsers/5
        //    [HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteTUser(int id)
        //{
        //    var tUser = await _context.TUsers.FindAsync(id);
        //    if (tUser == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.TUsers.Remove(tUser);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool TUserExists(int id)
        {
            return _context.TUsers.Any(e => e.FUserId == id);
        }
    }
}
