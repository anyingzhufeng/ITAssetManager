using ITAssetManager.API.DTOs;
using ITAssetManager.API.Services;
using ITAssetManager.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISystemSettingRepository _repo;
    private readonly IFeishuService _feishuService;

    public SettingsController(ISystemSettingRepository repo, IFeishuService feishuService)
    {
        _repo = repo;
        _feishuService = feishuService;
    }

    /// <summary>
    /// 获取所有设置
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<SettingsResponse>> GetAll()
    {
        var settings = await _repo.GetAllAsync();
        // 隐藏 Secret 敏感信息
        if (settings.ContainsKey("FeishuAppSecret") && !string.IsNullOrEmpty(settings["FeishuAppSecret"]))
            settings["FeishuAppSecret"] = "****" + settings["FeishuAppSecret"]?[^4..];
        return Ok(new SettingsResponse(settings));
    }

    /// <summary>
    /// 批量更新设置
    /// </summary>
    [HttpPut]
    public async Task<ActionResult> Update([FromBody] SettingsUpdateRequest req)
    {
        foreach (var (key, value) in req.Settings)
        {
            await _repo.SetValueAsync(key, value);
        }
        return Ok(new { message = "设置已保存" });
    }

    /// <summary>
    /// 测试飞书连接
    /// </summary>
    [HttpPost("test-feishu")]
    public async Task<ActionResult> TestFeishu()
    {
        var appId = await _repo.GetValueAsync("FeishuAppId");
        var appSecret = await _repo.GetValueAsync("FeishuAppSecret");

        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            return BadRequest(new { message = "请先填写飞书 App ID 和 App Secret" });

        // 尝试获取 tenant_access_token 验证配置
        try
        {
            var client = new HttpClient();
            var resp = await client.PostAsJsonAsync(
                "https://open.feishu.cn/open-apis/auth/v3/tenant_access_token/internal",
                new { app_id = appId, app_secret = appSecret });

            if (resp.IsSuccessStatusCode)
            {
                var result = await resp.Content.ReadFromJsonAsync<FeishuTestResponse>();
                if (result?.Code == 0)
                    return Ok(new { message = "✅ 飞书连接成功！", expire = result.Expire });
            }
            return BadRequest(new { message = "❌ 飞书连接失败，请检查 App ID / Secret" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"❌ 连接异常：{ex.Message}" });
        }
    }
}

public class FeishuTestResponse
{
    public int Code { get; set; }
    public string? Msg { get; set; }
    public int? Expire { get; set; }
}
