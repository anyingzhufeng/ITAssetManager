using ITAssetManager.API.DTOs;
using ITAssetManager.Core.Entities;
using ITAssetManager.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentRepository _repo;
    public DepartmentsController(IDepartmentRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentResponse>>> GetAll()
    {
        var depts = await _repo.GetAllAsync();
        return Ok(depts.Select(d => new DepartmentResponse(d.Id, d.Name, d.Code, d.ParentId, d.ManagerId, d.CreatedAt)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentResponse>> GetById(string id)
    {
        var d = await _repo.GetByIdAsync(id);
        if (d == null) return NotFound();
        return Ok(new DepartmentResponse(d.Id, d.Name, d.Code, d.ParentId, d.ManagerId, d.CreatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentResponse>> Create(DepartmentCreateRequest req)
    {
        var dept = new Department { Name = req.Name, Code = req.Code, ParentId = req.ParentId };
        dept = await _repo.CreateAsync(dept);
        return CreatedAtAction(nameof(GetById), new { id = dept.Id },
            new DepartmentResponse(dept.Id, dept.Name, dept.Code, dept.ParentId, dept.ManagerId, dept.CreatedAt));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, DepartmentUpdateRequest req)
    {
        var dept = await _repo.GetByIdAsync(id);
        if (dept == null) return NotFound();
        if (req.Name != null) dept.Name = req.Name;
        if (req.Code != null) dept.Code = req.Code;
        if (req.ManagerId != null) dept.ManagerId = req.ManagerId;
        await _repo.UpdateAsync(dept);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;
    public UsersController(IUserRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserResponse>>> Search(
        [FromQuery] string? keyword, [FromQuery] string? departmentId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (items, total) = await _repo.SearchAsync(keyword, departmentId, page, pageSize);
        return Ok(new PagedResponse<UserResponse>(
            items.Select(u => new UserResponse(u.Id, u.Name, u.EmployeeNo, u.Email, u.Phone, u.DepartmentId, u.Department?.Name, u.FeishuUserId, u.IsActive, u.CreatedAt)),
            total, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetById(string id)
    {
        var u = await _repo.GetByIdAsync(id);
        if (u == null) return NotFound();
        return Ok(new UserResponse(u.Id, u.Name, u.EmployeeNo, u.Email, u.Phone, u.DepartmentId, u.Department?.Name, u.FeishuUserId, u.IsActive, u.CreatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(UserCreateRequest req)
    {
        var user = new User { Name = req.Name, EmployeeNo = req.EmployeeNo, Email = req.Email, Phone = req.Phone, DepartmentId = req.DepartmentId };
        user = await _repo.CreateAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            new UserResponse(user.Id, user.Name, user.EmployeeNo, user.Email, user.Phone, user.DepartmentId, null, user.FeishuUserId, user.IsActive, user.CreatedAt));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, UserUpdateRequest req)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return NotFound();
        if (req.Name != null) user.Name = req.Name;
        if (req.Email != null) user.Email = req.Email;
        if (req.Phone != null) user.Phone = req.Phone;
        if (req.DepartmentId != null) user.DepartmentId = req.DepartmentId;
        if (req.IsActive.HasValue) user.IsActive = req.IsActive.Value;
        await _repo.UpdateAsync(user);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id) { await _repo.DeleteAsync(id); return NoContent(); }
}

[ApiController]
[Route("api/[controller]")]
public class SoftwareLicensesController : ControllerBase
{
    private readonly ISoftwareLicenseRepository _repo;
    public SoftwareLicensesController(ISoftwareLicenseRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<SoftwareLicenseResponse>>> Search([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (items, total) = await _repo.SearchAsync(keyword, page, pageSize);
        return Ok(new PagedResponse<SoftwareLicenseResponse>(
            items.Select(l => new SoftwareLicenseResponse(l.Id, l.Name, l.Version, l.LicenseKey, l.TotalLicenses, l.UsedLicenses, l.ExpiryDate, l.Vendor, l.Category, l.CreatedAt, l.UpdatedAt)),
            total, page, pageSize));
    }

    [HttpPost]
    public async Task<ActionResult<SoftwareLicenseResponse>> Create(SoftwareLicenseCreateRequest req)
    {
        var license = new SoftwareLicense { Name = req.Name, Version = req.Version, LicenseKey = req.LicenseKey, TotalLicenses = req.TotalLicenses, ExpiryDate = req.ExpiryDate, Vendor = req.Vendor, Category = req.Category };
        license = await _repo.CreateAsync(license);
        return Ok(new SoftwareLicenseResponse(license.Id, license.Name, license.Version, license.LicenseKey, license.TotalLicenses, license.UsedLicenses, license.ExpiryDate, license.Vendor, license.Category, license.CreatedAt, license.UpdatedAt));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, SoftwareLicenseUpdateRequest req)
    {
        var license = await _repo.GetByIdAsync(id);
        if (license == null) return NotFound();
        if (req.Name != null) license.Name = req.Name;
        if (req.Version != null) license.Version = req.Version;
        if (req.LicenseKey != null) license.LicenseKey = req.LicenseKey;
        if (req.TotalLicenses.HasValue) license.TotalLicenses = req.TotalLicenses.Value;
        if (req.ExpiryDate.HasValue) license.ExpiryDate = req.ExpiryDate.Value;
        if (req.Vendor != null) license.Vendor = req.Vendor;
        if (req.Category != null) license.Category = req.Category;
        await _repo.UpdateAsync(license);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id) { await _repo.DeleteAsync(id); return NoContent(); }
}

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardRepository _repo;
    public DashboardController(IDashboardRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> GetDashboard()
    {
        var recentLogs = await _repo.GetRecentLogsAsync(20);
        return Ok(new DashboardResponse(
            await _repo.GetTotalAssetsAsync(),
            await _repo.GetTotalDepartmentsAsync(),
            await _repo.GetTotalUsersAsync(),
            await _repo.GetTotalLicensesAsync(),
            await _repo.GetAssetsByCategoryAsync(),
            await _repo.GetAssetsByStatusAsync(),
            await _repo.GetAssetsByDepartmentAsync(),
            recentLogs.Select(l => new AssetLogResponse(l.Id, l.AssetId, l.Asset?.Name ?? "", l.Action.ToString(), l.OperatorId, l.Details, l.CreatedAt))
        ));
    }
}
