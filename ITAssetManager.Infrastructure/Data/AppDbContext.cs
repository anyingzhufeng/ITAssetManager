using ITAssetManager.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SoftwareLicense> SoftwareLicenses => Set<SoftwareLicense>();
    public DbSet<AssetLog> AssetLogs => Set<AssetLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Asset>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AssetTag).IsUnique();
            e.Property(x => x.AssetTag).IsRequired().HasMaxLength(50);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.PurchasePrice).HasPrecision(18, 2);
            e.HasOne(x => x.Department).WithMany(d => d.Assets).HasForeignKey(x => x.DepartmentId);
            e.HasOne(x => x.AssignedUser).WithMany(u => u.Assets).HasForeignKey(x => x.AssignedUserId);
        });

        modelBuilder.Entity<Department>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Code).IsRequired().HasMaxLength(20);
            e.HasOne(x => x.Parent).WithMany(d => d.Children).HasForeignKey(x => x.ParentId);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.EmployeeNo).IsUnique();
            e.Property(x => x.Name).IsRequired().HasMaxLength(50);
            e.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(20);
            e.Property(x => x.Role).HasMaxLength(20);
            e.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId);
        });

        modelBuilder.Entity<SoftwareLicense>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<AssetLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Asset).WithMany(a => a.Logs).HasForeignKey(x => x.AssetId);
            e.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<SystemSetting>(e =>
        {
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(100);
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = "dept-it", Name = "IT部门", Code = "IT", CreatedAt = new DateTime(2026, 1, 1) },
            new Department { Id = "dept-hr", Name = "人力资源部", Code = "HR", CreatedAt = new DateTime(2026, 1, 1) },
            new Department { Id = "dept-fin", Name = "财务部", Code = "FIN", CreatedAt = new DateTime(2026, 1, 1) },
            new Department { Id = "dept-mkt", Name = "市场部", Code = "MKT", CreatedAt = new DateTime(2026, 1, 1) }
        );

        // 默认管理员：admin / admin123（SHA256哈希）
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = "user-admin",
                Name = "系统管理员",
                EmployeeNo = "ADMIN001",
                Email = "admin@company.com",
                DepartmentId = "dept-it",
                PasswordHash = "100ec42cd1bdcea3a20fca9f5270f2b076f5f443ea4c3f256e0a6bc896bec104", // admin123
                Role = "Admin",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );

        // 默认系统设置
        modelBuilder.Entity<SystemSetting>().HasData(
            new SystemSetting { Key = "FeishuAppId", Value = "", Description = "飞书应用 App ID" },
            new SystemSetting { Key = "FeishuAppSecret", Value = "", Description = "飞书应用 App Secret" },
            new SystemSetting { Key = "SystemName", Value = "IT资产管理系统", Description = "系统名称" },
            new SystemSetting { Key = "LoginMode", Value = "password", Description = "登录模式：password / feishu" }
        );
    }
}
