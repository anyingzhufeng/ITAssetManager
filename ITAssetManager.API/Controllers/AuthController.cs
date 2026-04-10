using System.Text;
using System.Security.Cryptography;
using ITAssetManager.API.DTOs;
using ITAssetManager.API.Services;
using ITAssetManager.Core.Entities;
using ITAssetManager.Core.Interfaces;
using ITAssetManager.Infrastructure.Repositories;
using ITAssetManager.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IFeishuService _feishuService;
    private readonly IUserRepository _userRepo;
    private readonly ISystemSettingRepository _settingsRepo;
    private readonly AppDbContext _db;

    public AuthController(
        ITokenService tokenService,
        IFeishuService feishuService,
        IUserRepository userRepo,
        ISystemSettingRepository settingsRepo,
        AppDbContext db)
    {
        _tokenService = tokenService;
        _feishuService = feishuService;
        _userRepo = userRepo;
        _settingsRepo = settingsRepo;
        _db = db;
    }

    /// <summary>
    /// 用户名密码登录（支持工号或邮箱，忽略大小写）
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        var username = req.Username.Trim();

        // 支持工号、邮箱、姓名登录（模糊匹配）
        var user = await _db.Users.Include(u => u.Department)
            .FirstOrDefaultAsync(u =>
                u.EmployeeNo.Contains(username) ||
                (u.Email != null && u.Email.Contains(username)) ||
                u.Name.Contains(username));

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return Unauthorized(new { message = "用户名或密码错误" });

        // 验证密码
        var inputHash = HashPassword(req.Password);
        if (inputHash != user.PasswordHash)
            return Unauthorized(new { message = "用户名或密码错误" });

        if (!user.IsActive)
            return Unauthorized(new { message = "账号已停用" });

        var token = _tokenService.GenerateToken(user);
        return Ok(new LoginResponse(token, MapUser(user)));
    }

    /// <summary>
    /// 飞书 SSO 登录
    /// </summary>
    [HttpPost("feishu")]
    public async Task<ActionResult<LoginResponse>> FeishuLogin([FromBody] FeishuLoginRequest req)
    {
        var feishuUser = await _feishuService.GetUserInfoAsync(req.Code);
        if (feishuUser?.OpenId == null)
            return BadRequest(new { message = "飞书授权失败" });

        var user = await _userRepo.GetByFeishuUserIdAsync(feishuUser.OpenId);

        if (user == null)
        {
            user = new User
            {
                Name = feishuUser.Name ?? "飞书用户",
                EmployeeNo = $"FS-{feishuUser.OpenId[^8..]}",
                Email = feishuUser.Email,
                Phone = feishuUser.Mobile,
                FeishuUserId = feishuUser.OpenId,
                Role = "User",
            };
            user = await _userRepo.CreateAsync(user);
        }

        var token = _tokenService.GenerateToken(user);
        return Ok(new LoginResponse(token, MapUser(user)));
    }

    /// <summary>
    /// 获取当前登录用户
    /// </summary>
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return NotFound();

        return Ok(MapUser(user));
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [HttpPost("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return NotFound();

        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            var oldHash = HashPassword(req.OldPassword);
            if (oldHash != user.PasswordHash)
                return BadRequest(new { message = "旧密码错误" });
        }

        user.PasswordHash = HashPassword(req.NewPassword);
        await _userRepo.UpdateAsync(user);

        return Ok(new { message = "密码修改成功" });
    }

    /// <summary>
    /// 飞书应用挂载配置
    /// </summary>
    [HttpGet("feishu-config")]
    public async Task<ActionResult> GetFeishuConfig()
    {
        var appId = await _settingsRepo.GetValueAsync("FeishuAppId");
        return Ok(new
        {
            appId = appId ?? "",
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            nonceStr = Guid.NewGuid().ToString("N")[..16],
        });
    }

    internal static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + "ITAsset_Salt_2026"));
        return Convert.ToHexString(bytes).ToLower();
    }

    private static UserResponse MapUser(User u) =>
        new(u.Id, u.Name, u.EmployeeNo, u.Email, u.Phone,
            u.DepartmentId, u.Department?.Name, u.FeishuUserId, u.IsActive, u.CreatedAt, u.Role);
}
