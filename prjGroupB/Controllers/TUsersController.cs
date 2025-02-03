using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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



        //查詢
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
                    FUserRankId=emp.FUserRankId,
                    FUserNickName=emp.FUserNickName,
                    FUserEmail=emp.FUserEmail,
                    FUserBirthday=emp.FUserBirthday,
                    FUserPhone=emp.FUserPhone,
                    FUserSex=emp.FUserSex,
                    FUserAddress=emp.FUserAddress,
                    FUserImage=emp.FUserImage,
                    FUserComeDate=emp.FUserComeDate,
                    FUserPassword=emp.FUserPassword
                }
                );
        }
        //查詢
        // GET: api/TUsers/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<TUserDTO> GetTUser(int id)
        {
            var tUser = await _context.TUsers.FindAsync(id);
            TUserDTO userDTO = null;
            if (tUser != null)
            {
                userDTO = new TUserDTO
                {
                    FUserId = tUser.FUserId,
                    FUserName = tUser.FUserName,
                    FUserRankId = tUser.FUserRankId,
                    FUserNickName = tUser.FUserNickName,
                    FUserEmail = tUser.FUserEmail,
                    FUserBirthday = tUser.FUserBirthday,
                    FUserPhone = tUser.FUserPhone,
                    FUserSex = tUser.FUserSex,
                    FUserAddress = tUser.FUserAddress,
                    FUserImage = tUser.FUserImage,
                    FUserComeDate = tUser.FUserComeDate,
                    FUserPassword = tUser.FUserPassword
                };
            }

            return userDTO;
        }


        //修改
        // PUT: api/TUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<string> PutTUser(int id, [FromBody]TUserDTO userDTO)
        {
            if (id != userDTO.FUserId)
            {
                return "修改紀錄失敗";
            }
            TUser user = await _context.TUsers.FindAsync(id);
            if (user == null)
            {
                return "修改失敗";
            }
            user.FUserId = userDTO.FUserId;
            user.FUserName = userDTO.FUserName;
            user.FUserRankId = userDTO.FUserRankId;
            user.FUserNickName = userDTO.FUserNickName;
            user.FUserEmail = userDTO.FUserEmail;
            user.FUserBirthday = userDTO.FUserBirthday;
            user.FUserPhone = userDTO.FUserPhone;
            user.FUserSex = userDTO.FUserSex;
            user.FUserAddress = userDTO.FUserAddress;
            user.FUserImage = userDTO.FUserImage;
            user.FUserComeDate = userDTO.FUserComeDate;
            user.FUserPassword = userDTO.FUserPassword;


            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
               
                    return "修改失敗";
                
            }

            return "修改成功";
        }


        //新增
        // POST: api/TUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<string> PostTUser([FromBody]TUserDTO userDTO)
        {
            TUser user = new TUser
            {
                FUserId = userDTO.FUserId,
                FUserName = userDTO.FUserName,
                FUserRankId = userDTO.FUserRankId,
                FUserNickName = userDTO.FUserNickName,
                FUserEmail = userDTO.FUserEmail,
                FUserBirthday = userDTO.FUserBirthday,
                FUserPhone = userDTO.FUserPhone,
                FUserSex = userDTO.FUserSex,
                FUserAddress = userDTO.FUserAddress,
                FUserImage = userDTO.FUserImage,
                FUserComeDate = userDTO.FUserComeDate,
                FUserPassword = userDTO.FUserPassword

            };

            _context.TUsers.Add(user);
            await _context.SaveChangesAsync();
            return $"註冊成功，會員編號:{user.FUserId}";
        }

        // DELETE: api/TUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTUser(int id)
        {
            var tUser = await _context.TUsers.FindAsync(id);
            if (tUser == null)
            {
                return NotFound();
            }

            _context.TUsers.Remove(tUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TUserExists(int id)
        {
            return _context.TUsers.Any(e => e.FUserId == id);
        }
    }
}
