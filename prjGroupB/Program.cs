using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using prjGroupB.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5062); // 開啟 HTTP
    options.ListenAnyIP(7112, listenOptions => listenOptions.UseHttps()); // 開啟 HTTPS
});

//Add services to the container.
//Database
builder.Services.AddDbContext<dbGroupBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbGroupB")));

//JWT
var secretKey = "b6t8fJH2WjwYgJt7XPTqVX37WYgKs8TZ"; // 先假設隨機字串
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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

        // 從 Cookie 中提取 Token
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("jwt_token"))
                {
                    context.Token = context.Request.Cookies["jwt_token"];
                }
                return Task.CompletedTask;
            }
        };
    });

// ✅ 修正 CORS 設定
const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:4200") // 允許 Angular 存取
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ 使用定義好的 CORS 原則
app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();