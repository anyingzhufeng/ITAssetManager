using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace ITAssetManager.API.Services;

public interface IFeishuService
{
    Task<FeishuUserInfo?> GetUserInfoAsync(string code);
    Task<string?> GetAccessTokenAsync(string code);
}

public class FeishuService : IFeishuService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public FeishuService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string?> GetAccessTokenAsync(string code)
    {
        var appId = _config["Feishu:AppId"];
        var appSecret = _config["Feishu:AppSecret"];

        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            return null;

        // 用 code 换 access_token
        var request = new
        {
            grant_type = "authorization_code",
            code,
            app_id = appId,
            app_secret = appSecret
        };

        var response = await _httpClient.PostAsJsonAsync(
            "https://open.feishu.cn/open-apis/auth/v2/token", request);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<FeishuTokenResponse>();
        return result?.AccessToken;
    }

    public async Task<FeishuUserInfo?> GetUserInfoAsync(string code)
    {
        var accessToken = await GetAccessTokenAsync(code);
        if (string.IsNullOrEmpty(accessToken))
            return null;

        // 获取用户信息
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync(
            "https://open.feishu.cn/open-apis/auth/v3/user/info");

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<FeishuUserResponse>();
        return result?.Data;
    }
}

// 飞书 API 响应模型
public class FeishuTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class FeishuUserResponse
{
    [JsonPropertyName("data")]
    public FeishuUserInfo? Data { get; set; }
}

public class FeishuUserInfo
{
    [JsonPropertyName("open_id")]
    public string? OpenId { get; set; }

    [JsonPropertyName("union_id")]
    public string? UnionId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }
}
