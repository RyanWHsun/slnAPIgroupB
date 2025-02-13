using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using prjGroupB.DTO;
using prjGroupB.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace prjGroupB.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly dbGroupBContext _context;
        private readonly string _secretKey = "b6t8fJH2WjwYgJt7XPTqVX37WYgKs8TZ";//這是JWT密匙

        public AuthController(dbGroupBContext context) {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginRequest request)
        {
            // 先確保請求不為 null，並且 Email & Password 皆有輸入
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Message = "請提供有效的 Email 和密碼" });
            }


            //想先用名碼比對就把這區註解拿掉
            //查詢使用者(舊的，無雜湊)
            //var user = _context.TUsers.SingleOrDefault(u => u.FUserEmail == request.Email && u.FUserPassword== request.Password);


            //想先用名碼比對就把這區註解掉
            //查詢使用者(先找Email,再找雜湊)
            var user = _context.TUsers.SingleOrDefault(u => u.FUserEmail == request.Email);
            //驗證使用者是否存在
            if (user == null)
            {
                return Unauthorized(new { Message = "登入失敗，帳號或密碼錯誤" });
            }
            //驗證密碼（BCrypt）
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.FUserPassword))
            {
                return Unauthorized(new { Message = "登入失敗，帳號或密碼錯誤" });
            }

            //擋住RankId == 2
            if (user.FUserRankId == 2)
            {
                return Unauthorized(new { Message = "登入失敗，此帳號已註銷，請洽客服" });  
            }

            //產生JWT token
            var token = GenerateJwtToken(user);

            //HttpOnly Cookie（防止 JavaScript 竊取）
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,// 本機開發時設為 false，正式環境設為 true
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(24)//一天後過期
            });
            return Ok(new { Message = "登入成功"});
        }


        [HttpPost("logout")]
        public IActionResult LogOut()
        {
            // 1️ 清除 HttpOnly Cookie
            //Response.Cookies.Append("jwt_token","",new CookieOptions
            //{
            //    HttpOnly = true,
            //    Secure = false, // 本機開發時設為 false，正式環境應設為 true
            //    SameSite = SameSiteMode.Lax,
            //    Path = "/", // 確保 Cookie 被刪除
            //    Expires = DateTime.UtcNow.AddYears(-1) // 立即讓 Cookie 過期
            //});

            Response.Cookies.Delete("jwt_token");


            // 2️ 返回成功訊息
            return Ok(new { Message = "登出成功" });
        }





        private string GenerateJwtToken(TUser user)
        {
            // 取得密鑰並建立簽名憑證
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 設定 Token 內的 Claims (用戶識別資訊，目前:FUserId、FUserName、)
            var claims = new List<Claim>
            {
                  new Claim(ClaimTypes.NameIdentifier, user.FUserId.ToString()),
                  new Claim(ClaimTypes.Name, user.FUserName ?? "未知用戶") // 避免 FUserName 為 null
            };

            // 產生 JWT Token
            var token = new JwtSecurityToken(
                issuer: "http://localhost:7112",
                audience: "http://localhost:4200",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),//Token在一天後過期
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
