using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using prjGroupB.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
//Database
builder.Services.AddDbContext<dbGroupBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbGroupB")));

//JWT
//var secretKey = "YourSuperSecretKey"; // 請使用更安全的密鑰
var secretKey = "b6t8fJH2WjwYgJt7XPTqVX37WYgKs8TZ";//先假設隨機字串符
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

//CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowCredentials().AllowAnyHeader().AllowAnyMethod();
    });
});
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowFrontend",
//        policy =>
//        {
//            policy.WithOrigins("http://localhost:4200")
//                  .AllowCredentials() // 允許攜帶 Cookie
//                  .AllowAnyMethod()
//                  .AllowAnyHeader();
//        });
//});






builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//CORS
app.UseCors();
//app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
