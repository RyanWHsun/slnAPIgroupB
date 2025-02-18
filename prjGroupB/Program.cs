using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using prjGroupB.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ? 註冊 ImageService
builder.Services.AddScoped<IImageService, ImageService>();

// ? 設定資料庫連線
builder.Services.AddDbContext<dbGroupBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbGroupB")));

// ? 設定 JWT 驗證
var secretKey = "b6t8fJH2WjwYgJt7XPTqVX37WYgKs8TZ"; // 測試密鑰
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost:7112",
            ValidAudience = "http://localhost:4200",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        // ? 確保從 Cookie 中提取 JWT Token
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context => {
                if (context.Request.Cookies.ContainsKey("jwt_token"))
                {
                    context.Token = context.Request.Cookies["jwt_token"];
                }
                return Task.CompletedTask;
            }
        };
    });

// ? 修正 CORS 設定
var MyAllowSpecificOrigins = "AllowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowCredentials()// 允許攜帶 Cookie
              .AllowAnyHeader()
              .AllowAnyMethod(); 
    });
});

// ? 註冊 Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ? 註冊 HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// ? 啟用 Swagger (僅限開發環境)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ? 確保 CORS 設定生效
app.UseCors(MyAllowSpecificOrigins);

// ? 啟用 HTTPS 重新導向
app.UseHttpsRedirection();

// ? 啟用 JWT 驗證
app.UseAuthentication();
app.UseAuthorization();

// ? 設定路由
app.MapControllers();

// ? 啟動應用程式
app.Run();
