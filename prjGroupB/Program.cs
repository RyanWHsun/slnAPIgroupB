using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using prjGroupB.Hubs;
using prjGroupB.DTO;
using prjGroupB.Models;
using System.Text;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// ✅ **檢查 ConnectionString**
var connectionString = builder.Configuration.GetConnectionString("dbGroupB");
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("❌ 錯誤: 無法讀取 ConnectionString 'dbGroupB'，請檢查 appsettings.json 是否正確！");
}

Console.WriteLine("🔹 ConnectionString: " + connectionString);

// ? 註冊 ImageService
builder.Services.AddScoped<IImageService, ImageService>();



// ? 設定資料庫連線
builder.Services.AddDbContext<dbGroupBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbGroupB")));
// 註冊SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

// ? 註冊 LinePayService（**改用 dbContext 來讀取資料庫**）
builder.Services.AddScoped<LinePayService>();

// ? 設定 JWT 驗證
var secretKey = "b6t8fJH2WjwYgJt7XPTqVX37WYgKs8TZ"; // 測試密鑰
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

        // ? 確保從 Cookie 中提取 JWT Token
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

    options.AddPolicy("AllowQRScan", policy =>
    {
        policy.AllowAnyOrigin() // 允許所有來源
              .WithMethods("PUT") 
              .WithMethods("GET")
              .AllowAnyHeader();
    });
});

// 註冊 ECPay 設定
builder.Services.Configure<ECPaySettings>(builder.Configuration.GetSection("ECPaySettings"));

// ? 註冊 Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ? 設定 Swagger 支援 JWT LINE PAY
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "請輸入你的 JWT Token，例如：Bearer {你的 Token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// ? 註冊 HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// ✅ **測試 API (確認是否成功連接資料庫)** LINE PAY
app.MapGet("/api/test-connection", async (dbGroupBContext context) =>
{
    try
    {
        var ordersCount = await context.TOrders.CountAsync();
        return Results.Ok($"✅ 連接成功！訂單數量: {ordersCount}");
    }
    catch (Exception ex)
    {
        return Results.Problem("❌ 連接失敗：" + ex.Message);
    }
});

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
//註冊SignalR Hub
app.MapHub<ChatHub>("/chatHub");

// 註冊 SignalR Hub
app.MapHub<OrderHub>("/orderHub");

// ? 設定路由
app.MapControllers();


// ? 啟動應用程式
app.Run();
