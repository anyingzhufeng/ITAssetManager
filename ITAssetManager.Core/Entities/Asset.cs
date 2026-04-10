using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManager.Core.Entities;

public enum AssetCategory
{
    Computer, Laptop, Server, NetworkDevice, Printer, Phone, Software, Monitor, Peripheral, Other
}

public enum AssetStatus
{
    InStock, InUse, Maintenance, Retired, Disposed
}

public enum AssetAction
{
    Create, Update, Assign, Return, Maintain, Dispose
}

public class Asset
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssetCategory Category { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.InStock;
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public string? Location { get; set; }
    public string? DepartmentId { get; set; }
    public string? AssignedUserId { get; set; }
    public string? IpAddress { get; set; }
    public string? MacAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public Department? Department { get; set; }
    public User? AssignedUser { get; set; }
    public ICollection<AssetLog> Logs { get; set; } = new List<AssetLog>();
}

public class Department
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public string? ManagerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Department? Parent { get; set; }
    public ICollection<Department> Children { get; set; } = new List<Department>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? DepartmentId { get; set; }
    public string? FeishuUserId { get; set; }
    public string? PasswordHash { get; set; } // 密码哈希（本地登录用）
    public string? Role { get; set; } = "User"; // Admin / User
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Department? Department { get; set; }
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}

public class SoftwareLicense
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? LicenseKey { get; set; }
    public int TotalLicenses { get; set; } = 1;
    public int UsedLicenses { get; set; } = 0;
    public DateTime? ExpiryDate { get; set; }
    public string? Vendor { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class AssetLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AssetId { get; set; } = string.Empty;
    public AssetAction Action { get; set; }
    public string? OperatorId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Asset? Asset { get; set; }
}

/// <summary>
/// 系统设置（KV 存储）
/// </summary>
public class SystemSetting
{
    public string Key { get; set; } = string.Empty;  // 主键
    public string? Value { get; set; }
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
