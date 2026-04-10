using System.Text;
using ITAssetManager.API.Services;
using ITAssetManager.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// === 数据库 ===
var dbProvider = builder.Configuration["Database:Provider"] ?? "SQLite";
string connStr;

if (dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
{
    var pgHost = builder.Configuration["Database:Host"] ?? "localhost";
    var pgPort = builder.Configuration["Database:Port"] ?? "5432";
    var pgDb = builder.Configuration["Database:Name"] ?? "itasset";
    var pgUser = builder.Configuration["Database:User"] ?? "postgres";
    var pgPassword = builder.Configuration["Database:Password"] ?? "postgres";
    connStr = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPassword}";
}
else
{
    connStr = $"Data Source={builder.Configuration["Database:SqlitePath"] ?? "data/itasset.db"}";
}

builder.Services.AddInfrastructure(dbProvider, connStr);

// === JWT 认证 ===
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "ITAssetManager_DefaultSecret_Key_2026!@#$%";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ITAssetManager",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ITAssetManager",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});
builder.Services.AddAuthorization();

// === 应用服务 ===
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddHttpClient<IFeishuService, FeishuService>();

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// === Controllers ===
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// === 自动建库 ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ITAssetManager.Infrastructure.Data.AppDbContext>();
    db.Database.EnsureCreated();
}

// === 中间件 ===
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// === 托管前端 ===
var frontendPath = Path.Combine(builder.Environment.ContentRootPath, "..", "frontend", "dist");
if (Directory.Exists(frontendPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
            Path.GetFullPath(frontendPath)),
        RequestPath = ""
    });
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
            Path.GetFullPath(frontendPath))
    });
}

app.Run();
