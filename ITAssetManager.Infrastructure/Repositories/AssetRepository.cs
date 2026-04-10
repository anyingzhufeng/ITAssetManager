using ITAssetManager.Core.Entities;
using ITAssetManager.Core.Interfaces;
using ITAssetManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Infrastructure.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _db;
    public AssetRepository(AppDbContext db) => _db = db;

    public async Task<Asset?> GetByIdAsync(string id) =>
        await _db.Assets.Include(a => a.Department).Include(a => a.AssignedUser).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Asset?> GetByAssetTagAsync(string assetTag) =>
        await _db.Assets.FirstOrDefaultAsync(a => a.AssetTag == assetTag);

    public async Task<(IEnumerable<Asset> Items, int Total)> SearchAsync(
        string? keyword, AssetCategory? category, AssetStatus? status,
        string? departmentId, int page, int pageSize)
    {
        var query = _db.Assets.Include(a => a.Department).Include(a => a.AssignedUser).AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(a => a.Name.Contains(keyword) || a.AssetTag.Contains(keyword) || (a.SerialNumber != null && a.SerialNumber.Contains(keyword)));
        if (category.HasValue)
            query = query.Where(a => a.Category == category.Value);
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(departmentId))
            query = query.Where(a => a.DepartmentId == departmentId);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<Asset> CreateAsync(Asset asset)
    {
        _db.Assets.Add(asset);
        await _db.SaveChangesAsync();
        return asset;
    }

    public async Task UpdateAsync(Asset asset)
    {
        asset.UpdatedAt = DateTime.UtcNow;
        _db.Assets.Update(asset);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var asset = await _db.Assets.FindAsync(id);
        if (asset != null) { _db.Assets.Remove(asset); await _db.SaveChangesAsync(); }
    }

    public async Task<IEnumerable<AssetLog>> GetLogsAsync(string assetId) =>
        await _db.AssetLogs.Where(l => l.AssetId == assetId).OrderByDescending(l => l.CreatedAt).ToListAsync();

    public async Task AddLogAsync(AssetLog log)
    {
        _db.AssetLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _db;
    public DepartmentRepository(AppDbContext db) => _db = db;

    public async Task<Department?> GetByIdAsync(string id) => await _db.Departments.FindAsync(id);
    public async Task<IEnumerable<Department>> GetAllAsync() => await _db.Departments.OrderBy(d => d.Name).ToListAsync();
    public async Task<IEnumerable<Department>> GetChildrenAsync(string parentId) => await _db.Departments.Where(d => d.ParentId == parentId).ToListAsync();

    public async Task<Department> CreateAsync(Department dept) { _db.Departments.Add(dept); await _db.SaveChangesAsync(); return dept; }
    public async Task UpdateAsync(Department dept) { _db.Departments.Update(dept); await _db.SaveChangesAsync(); }
    public async Task DeleteAsync(string id) { var d = await _db.Departments.FindAsync(id); if (d != null) { _db.Departments.Remove(d); await _db.SaveChangesAsync(); } }
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(string id) => await _db.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.Id == id);
    public async Task<User?> GetByFeishuUserIdAsync(string feishuUserId) => await _db.Users.FirstOrDefaultAsync(u => u.FeishuUserId == feishuUserId);

    public async Task<(IEnumerable<User> Items, int Total)> SearchAsync(string? keyword, string? departmentId, int page, int pageSize)
    {
        var query = _db.Users.Include(u => u.Department).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(u => u.Name.Contains(keyword) || u.EmployeeNo.Contains(keyword));
        if (!string.IsNullOrWhiteSpace(departmentId)) query = query.Where(u => u.DepartmentId == departmentId);
        var total = await query.CountAsync();
        var items = await query.OrderBy(u => u.EmployeeNo).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<User> CreateAsync(User user) { _db.Users.Add(user); await _db.SaveChangesAsync(); return user; }
    public async Task UpdateAsync(User user) { _db.Users.Update(user); await _db.SaveChangesAsync(); }
    public async Task DeleteAsync(string id) { var u = await _db.Users.FindAsync(id); if (u != null) { _db.Users.Remove(u); await _db.SaveChangesAsync(); } }
}

public class SoftwareLicenseRepository : ISoftwareLicenseRepository
{
    private readonly AppDbContext _db;
    public SoftwareLicenseRepository(AppDbContext db) => _db = db;

    public async Task<SoftwareLicense?> GetByIdAsync(string id) => await _db.SoftwareLicenses.FindAsync(id);

    public async Task<(IEnumerable<SoftwareLicense> Items, int Total)> SearchAsync(string? keyword, int page, int pageSize)
    {
        var query = _db.SoftwareLicenses.AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(l => l.Name.Contains(keyword) || (l.Vendor != null && l.Vendor.Contains(keyword)));
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<SoftwareLicense> CreateAsync(SoftwareLicense license) { _db.SoftwareLicenses.Add(license); await _db.SaveChangesAsync(); return license; }
    public async Task UpdateAsync(SoftwareLicense license) { license.UpdatedAt = DateTime.UtcNow; _db.SoftwareLicenses.Update(license); await _db.SaveChangesAsync(); }
    public async Task DeleteAsync(string id) { var l = await _db.SoftwareLicenses.FindAsync(id); if (l != null) { _db.SoftwareLicenses.Remove(l); await _db.SaveChangesAsync(); } }
}

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _db;
    public DashboardRepository(AppDbContext db) => _db = db;

    public async Task<int> GetTotalAssetsAsync() => await _db.Assets.CountAsync();

    public async Task<Dictionary<string, int>> GetAssetsByCategoryAsync() =>
        await _db.Assets.GroupBy(a => a.Category).ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

    public async Task<Dictionary<string, int>> GetAssetsByStatusAsync() =>
        await _db.Assets.GroupBy(a => a.Status).ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

    public async Task<Dictionary<string, int>> GetAssetsByDepartmentAsync() =>
        await _db.Assets.Where(a => a.DepartmentId != null)
            .GroupBy(a => a.Department!.Name).ToDictionaryAsync(g => g.Key, g => g.Count());

    public async Task<IEnumerable<AssetLog>> GetRecentLogsAsync(int count) =>
        await _db.AssetLogs.Include(l => l.Asset).OrderByDescending(l => l.CreatedAt).Take(count).ToListAsync();

    public async Task<int> GetTotalDepartmentsAsync() => await _db.Departments.CountAsync();
    public async Task<int> GetTotalUsersAsync() => await _db.Users.CountAsync();
    public async Task<int> GetTotalLicensesAsync() => await _db.SoftwareLicenses.SumAsync(l => l.TotalLicenses);
}
