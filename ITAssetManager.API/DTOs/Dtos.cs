using ITAssetManager.Core.Entities;

namespace ITAssetManager.API.DTOs;

// Asset DTOs
public record AssetCreateRequest(
    string AssetTag, string Name, AssetCategory Category,
    string? Description = null, string? Brand = null, string? Model = null,
    string? SerialNumber = null, AssetStatus? Status = null,
    DateTime? PurchaseDate = null, decimal? PurchasePrice = null,
    DateTime? WarrantyExpiry = null, string? Location = null,
    string? DepartmentId = null, string? IpAddress = null,
    string? MacAddress = null, string? Notes = null);

public record AssetUpdateRequest(
    string? Name = null, string? Description = null, string? Brand = null,
    string? Model = null, string? Location = null, string? DepartmentId = null,
    string? IpAddress = null, string? MacAddress = null, string? Notes = null);

public record AssetAssignRequest(string UserId);
public record AssetMaintainRequest(string Reason);

public record AssetResponse(
    string Id, string AssetTag, string Name, string? Description, string Category,
    string? Brand, string? Model, string? SerialNumber, string Status,
    DateTime? PurchaseDate, decimal? PurchasePrice, DateTime? WarrantyExpiry,
    string? Location, string? DepartmentId, string? DepartmentName,
    string? AssignedUserId, string? AssignedUserName,
    string? IpAddress, string? MacAddress, DateTime CreatedAt, DateTime UpdatedAt, string? Notes);

// Department DTOs
public record DepartmentCreateRequest(string Name, string Code, string? ParentId = null);
public record DepartmentUpdateRequest(string? Name = null, string? Code = null, string? ManagerId = null);
public record DepartmentResponse(string Id, string Name, string Code, string? ParentId, string? ManagerId, DateTime CreatedAt);

// User DTOs
public record UserCreateRequest(string Name, string EmployeeNo, string? Email = null, string? Phone = null, string? DepartmentId = null, string? Password = null);
public record UserUpdateRequest(string? Name = null, string? Email = null, string? Phone = null, string? DepartmentId = null, bool? IsActive = null, string? Role = null);
public record UserResponse(string Id, string Name, string EmployeeNo, string? Email, string? Phone, string? DepartmentId, string? DepartmentName, string? FeishuUserId, bool IsActive, DateTime CreatedAt, string? Role = null);

// Software License DTOs
public record SoftwareLicenseCreateRequest(string Name, string? Version = null, string? LicenseKey = null, int TotalLicenses = 1, DateTime? ExpiryDate = null, string? Vendor = null, string? Category = null);
public record SoftwareLicenseUpdateRequest(string? Name = null, string? Version = null, string? LicenseKey = null, int? TotalLicenses = null, DateTime? ExpiryDate = null, string? Vendor = null, string? Category = null);
public record SoftwareLicenseResponse(string Id, string Name, string? Version, string? LicenseKey, int TotalLicenses, int UsedLicenses, DateTime? ExpiryDate, string? Vendor, string? Category, DateTime CreatedAt, DateTime UpdatedAt);

// Auth DTOs
public record LoginRequest(string Username, string Password);
public record FeishuLoginRequest(string Code);
public record LoginResponse(string Token, UserResponse User);
public record ChangePasswordRequest(string OldPassword, string NewPassword);

// Settings DTOs
public record SettingsUpdateRequest(Dictionary<string, string> Settings);
public record SettingsResponse(Dictionary<string, string?> Settings);

// Dashboard DTOs
public record DashboardResponse(
    int TotalAssets, int TotalDepartments, int TotalUsers, int TotalLicenses,
    Dictionary<string, int> AssetsByCategory, Dictionary<string, int> AssetsByStatus,
    Dictionary<string, int> AssetsByDepartment, IEnumerable<AssetLogResponse> RecentLogs);

public record AssetLogResponse(string Id, string AssetId, string AssetName, string Action, string? OperatorId, string? Details, DateTime CreatedAt);

// Paged Response
public record PagedResponse<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
