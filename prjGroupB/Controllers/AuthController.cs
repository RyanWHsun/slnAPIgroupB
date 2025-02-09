using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using prjGroupB.DTO;
using prjGroupB.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly dbGroupBContext _context;
        private readonly string _secretKey = "b6t8fJH2WjwYgJt7XPTqVX37WYgKs8TZ";

        public AuthController(dbGroupBContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginRequest request)
        {
            var user = _context.TUsers.SingleOrDefault(u => u.FUserEmail == request.Email &&u.FUserPassword== request.Password);

            //JWT token
            var token = GenerateJwtToken(user);

            //HttpOnly Cookie
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(24)
            });
            return Ok(new { Message = "Login successful", token=token});
        }

        private string GenerateJwtToken(TUser user)
        {
            //改的
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "無法生成 JWT，因為 user 為 null");
            }
            



            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //改的
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.FUserId.ToString()),
        new Claim(ClaimTypes.Name, user.FUserName ?? "未知用戶") // 避免 FUserName 為 null
    };


            //var claims = new[]
            //{
            //new Claim(ClaimTypes.NameIdentifier, user.FUserId.ToString()),
            //new Claim(ClaimTypes.Name, user.FUserName)
            //};

            var token = new JwtSecurityToken(
                issuer: "http://localhost:7112",
                audience: "http://localhost:4200",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
