using ITAssetManager.API.DTOs;
using ITAssetManager.Core.Entities;
using ITAssetManager.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IAssetRepository _repo;
    public AssetsController(IAssetRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AssetResponse>>> Search(
        [FromQuery] string? keyword, [FromQuery] AssetCategory? category,
        [FromQuery] AssetStatus? status, [FromQuery] string? departmentId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (items, total) = await _repo.SearchAsync(keyword, category, status, departmentId, page, pageSize);
        return Ok(new PagedResponse<AssetResponse>(
            items.Select(MapToResponse), total, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssetResponse>> GetById(string id)
    {
        var asset = await _repo.GetByIdAsync(id);
        if (asset == null) return NotFound();
        return Ok(MapToResponse(asset));
    }

    [HttpPost]
    public async Task<ActionResult<AssetResponse>> Create(AssetCreateRequest req)
    {
        if (await _repo.GetByAssetTagAsync(req.AssetTag) != null)
            return BadRequest(new { message = $"资产编号 {req.AssetTag} 已存在" });

        var asset = new Asset
        {
            AssetTag = req.AssetTag, Name = req.Name, Category = req.Category,
            Description = req.Description, Brand = req.Brand, Model = req.Model,
            SerialNumber = req.SerialNumber, PurchaseDate = req.PurchaseDate,
            PurchasePrice = req.PurchasePrice, WarrantyExpiry = req.WarrantyExpiry,
            Location = req.Location, DepartmentId = req.DepartmentId,
            IpAddress = req.IpAddress, MacAddress = req.MacAddress, Notes = req.Notes
        };
        asset = await _repo.CreateAsync(asset);
        await _repo.AddLogAsync(new AssetLog { AssetId = asset.Id, Action = AssetAction.Create, Details = "创建资产" });

        var created = await _repo.GetByIdAsync(asset.Id);
        return CreatedAtAction(nameof(GetById), new { id = asset.Id }, MapToResponse(created!));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AssetResponse>> Update(string id, AssetUpdateRequest req)
    {
        var asset = await _repo.GetByIdAsync(id);
        if (asset == null) return NotFound();

        if (req.Name != null) asset.Name = req.Name;
        if (req.Description != null) asset.Description = req.Description;
        if (req.Brand != null) asset.Brand = req.Brand;
        if (req.Model != null) asset.Model = req.Model;
        if (req.Location != null) asset.Location = req.Location;
        if (req.DepartmentId != null) asset.DepartmentId = req.DepartmentId;
        if (req.IpAddress != null) asset.IpAddress = req.IpAddress;
        if (req.MacAddress != null) asset.MacAddress = req.MacAddress;
        if (req.Notes != null) asset.Notes = req.Notes;

        await _repo.UpdateAsync(asset);
        await _repo.AddLogAsync(new AssetLog { AssetId = id, Action = AssetAction.Update, Details = "更新资产信息" });

        var updated = await _repo.GetByIdAsync(id);
        return Ok(MapToResponse(updated!));
    }

    [HttpPost("{id}/assign")]
    public async Task<ActionResult> Assign(string id, AssetAssignRequest req)
    {
        var asset = await _repo.GetByIdAsync(id);
        if (asset == null) return NotFound();
        if (asset.Status != AssetStatus.InStock) return BadRequest(new { message = "只有在库状态的资产才能分配" });

        asset.AssignedUserId = req.UserId;
        asset.Status = AssetStatus.InUse;
        await _repo.UpdateAsync(asset);
        await _repo.AddLogAsync(new AssetLog { AssetId = id, Action = AssetAction.Assign, OperatorId = req.UserId, Details = $"分配给用户 {req.UserId}" });

        return Ok(new { message = "分配成功" });
    }

    [HttpPost("{id}/return")]
    public async Task<ActionResult> Return(string id)
    {
        var asset = await _repo.GetByIdAsync(id);
        if (asset == null) return NotFound();

        var userId = asset.AssignedUserId;
        asset.AssignedUserId = null;
        asset.Status = AssetStatus.InStock;
        await _repo.UpdateAsync(asset);
        await _repo.AddLogAsync(new AssetLog { AssetId = id, Action = AssetAction.Return, OperatorId = userId, Details = "归还资产" });

        return Ok(new { message = "归还成功" });
    }

    [HttpPost("{id}/maintain")]
    public async Task<ActionResult> Maintain(string id, AssetMaintainRequest req)
    {
        var asset = await _repo.GetByIdAsync(id);
        if (asset == null) return NotFound();

        asset.Status = AssetStatus.Maintenance;
        await _repo.UpdateAsync(asset);
        await _repo.AddLogAsync(new AssetLog { AssetId = id, Action = AssetAction.Maintain, Details = req.Reason });

        return Ok(new { message = "已标记为维修中" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/logs")]
    public async Task<ActionResult<IEnumerable<AssetLogResponse>>> GetLogs(string id)
    {
        var logs = await _repo.GetLogsAsync(id);
        return Ok(logs.Select(l => new AssetLogResponse(l.Id, l.AssetId, "", l.Action.ToString(), l.OperatorId, l.Details, l.CreatedAt)));
    }

    private static AssetResponse MapToResponse(Asset a) => new(
        a.Id, a.AssetTag, a.Name, a.Description, a.Category.ToString(),
        a.Brand, a.Model, a.SerialNumber, a.Status.ToString(),
        a.PurchaseDate, a.PurchasePrice, a.WarrantyExpiry,
        a.Location, a.DepartmentId, a.Department?.Name,
        a.AssignedUserId, a.AssignedUser?.Name,
        a.IpAddress, a.MacAddress, a.CreatedAt, a.UpdatedAt, a.Notes
    );
}
