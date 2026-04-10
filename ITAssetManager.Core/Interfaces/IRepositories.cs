using ITAssetManager.Core.Entities;

namespace ITAssetManager.Core.Interfaces;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(string id);
    Task<Asset?> GetByAssetTagAsync(string assetTag);
    Task<(IEnumerable<Asset> Items, int Total)> SearchAsync(
        string? keyword, AssetCategory? category, AssetStatus? status,
        string? departmentId, int page, int pageSize);
    Task<Asset> CreateAsync(Asset asset);
    Task UpdateAsync(Asset asset);
    Task DeleteAsync(string id);
    Task<IEnumerable<AssetLog>> GetLogsAsync(string assetId);
    Task AddLogAsync(AssetLog log);
}

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(string id);
    Task<IEnumerable<Department>> GetAllAsync();
    Task<IEnumerable<Department>> GetChildrenAsync(string parentId);
    Task<Department> CreateAsync(Department dept);
    Task UpdateAsync(Department dept);
    Task DeleteAsync(string id);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByFeishuUserIdAsync(string feishuUserId);
    Task<(IEnumerable<User> Items, int Total)> SearchAsync(
        string? keyword, string? departmentId, int page, int pageSize);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(string id);
}

public interface ISoftwareLicenseRepository
{
    Task<SoftwareLicense?> GetByIdAsync(string id);
    Task<(IEnumerable<SoftwareLicense> Items, int Total)> SearchAsync(
        string? keyword, int page, int pageSize);
    Task<SoftwareLicense> CreateAsync(SoftwareLicense license);
    Task UpdateAsync(SoftwareLicense license);
    Task DeleteAsync(string id);
}

public interface IDashboardRepository
{
    Task<int> GetTotalAssetsAsync();
    Task<Dictionary<string, int>> GetAssetsByCategoryAsync();
    Task<Dictionary<string, int>> GetAssetsByStatusAsync();
    Task<Dictionary<string, int>> GetAssetsByDepartmentAsync();
    Task<IEnumerable<AssetLog>> GetRecentLogsAsync(int count);
    Task<int> GetTotalDepartmentsAsync();
    Task<int> GetTotalUsersAsync();
    Task<int> GetTotalLicensesAsync();
}
