using ITAssetManager.Core.Entities;
using ITAssetManager.Core.Interfaces;
using ITAssetManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Infrastructure.Repositories;

public interface ISystemSettingRepository
{
    Task<string?> GetValueAsync(string key);
    Task SetValueAsync(string key, string value);
    Task<Dictionary<string, string?>> GetAllAsync();
}

public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly AppDbContext _db;
    public SystemSettingRepository(AppDbContext db) => _db = db;

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await _db.SystemSettings.FindAsync(key);
        return setting?.Value;
    }

    public async Task SetValueAsync(string key, string value)
    {
        var setting = await _db.SystemSettings.FindAsync(key);
        if (setting != null)
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
        }
        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string?>> GetAllAsync()
    {
        return await _db.SystemSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
    }
}
