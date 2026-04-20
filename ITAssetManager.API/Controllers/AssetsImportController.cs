using ClosedXML.Excel;
using ITAssetManager.API.DTOs;
using ITAssetManager.Core.Entities;
using ITAssetManager.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsImportController : ControllerBase
{
    private readonly AppDbContext _db;
    private static readonly Dictionary<string, AssetCategory> CategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        {"台式电脑", AssetCategory.Computer}, {"Computer", AssetCategory.Computer},
        {"笔记本", AssetCategory.Laptop}, {"Laptop", AssetCategory.Laptop},
        {"服务器", AssetCategory.Server}, {"Server", AssetCategory.Server},
        {"网络设备", AssetCategory.NetworkDevice}, {"NetworkDevice", AssetCategory.NetworkDevice},
        {"打印机", AssetCategory.Printer}, {"Printer", AssetCategory.Printer},
        {"手机", AssetCategory.Phone}, {"电话", AssetCategory.Phone}, {"Phone", AssetCategory.Phone},
        {"软件", AssetCategory.Software}, {"Software", AssetCategory.Software},
        {"显示器", AssetCategory.Monitor}, {"Monitor", AssetCategory.Monitor},
        {"外设", AssetCategory.Peripheral}, {"Peripheral", AssetCategory.Peripheral},
        {"其他", AssetCategory.Other}, {"Other", AssetCategory.Other},
    };

    private static readonly Dictionary<string, AssetStatus> StatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        {"在库", AssetStatus.InStock}, {"InStock", AssetStatus.InStock},
        {"使用中", AssetStatus.InUse}, {"InUse", AssetStatus.InUse},
        {"维修中", AssetStatus.Maintenance}, {"Maintenance", AssetStatus.Maintenance},
        {"已报废", AssetStatus.Retired}, {"Retired", AssetStatus.Retired},
        {"已处置", AssetStatus.Disposed}, {"Disposed", AssetStatus.Disposed},
    };

    public AssetsImportController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 下载 Excel 导入模板
    /// </summary>
    [HttpGet("template")]
    public IActionResult DownloadTemplate()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("资产导入模板");

        // 表头
        var headers = new[] { "资产编号*", "名称*", "分类", "品牌", "型号", "序列号", "状态", "采购日期", "采购价格", "过保日期", "位置", "部门名称", "备注" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // 示例数据
        var examples = new[]
        {
            new[] { "NB-001", "联想 ThinkPad X1 Carbon", "笔记本", "联想", "X1 Carbon Gen 11", "", "使用中", "2025-06-15", "12999", "2028-06-15", "苏州总部3楼", "IT部门", "管理员工位" },
            new[] { "PC-001", "戴尔 OptiPlex 7010", "台式电脑", "戴尔", "OptiPlex 7010", "SN12345", "在库", "2024-09-15", "5499", "2027-09-15", "仓库B", "财务部", "" },
            new[] { "SRV-001", "戴尔 PowerEdge R750", "服务器", "戴尔", "PowerEdge R750", "", "使用中", "2024-01-15", "89999", "2029-01-15", "苏州机房", "IT部门", "主数据库服务器" },
        };

        for (int r = 0; r < examples.Length; r++)
        {
            for (int c = 0; c < examples[r].Length; c++)
            {
                ws.Cell(r + 2, c + 1).Value = examples[r][c];
            }
        }

        // 说明 sheet
        var ws2 = wb.Worksheets.Add("说明");
        ws2.Cell(1, 1).Value = "分类可选值";
        ws2.Cell(1, 1).Style.Font.Bold = true;
        var cats = new[] { "台式电脑", "笔记本", "服务器", "网络设备", "打印机", "手机", "软件", "显示器", "外设", "其他" };
        for (int i = 0; i < cats.Length; i++)
            ws2.Cell(i + 2, 1).Value = cats[i];

        ws2.Cell(1, 3).Value = "状态可选值";
        ws2.Cell(1, 3).Style.Font.Bold = true;
        var statuses = new[] { "在库", "使用中", "维修中", "已报废", "已处置" };
        for (int i = 0; i < statuses.Length; i++)
            ws2.Cell(i + 2, 3).Value = statuses[i];

        ws2.Cell(1, 5).Value = "部门名称（需与系统中一致）";
        ws2.Cell(1, 5).Style.Font.Bold = true;
        var depts = _db.Departments.ToList();
        for (int i = 0; i < depts.Count; i++)
        {
            ws2.Cell(i + 2, 5).Value = depts[i].Name;
            ws2.Cell(i + 2, 6).Value = depts[i].Id;
        }

        ws2.Cell(1, 8).Value = "注意事项";
        ws2.Cell(1, 8).Style.Font.Bold = true;
        ws2.Cell(2, 8).Value = "1. 资产编号*和名称*为必填项";
        ws2.Cell(3, 8).Value = "2. 资产编号不能重复";
        ws2.Cell(4, 8).Value = "3. 分类/状态不填则使用默认值";
        ws2.Cell(5, 8).Value = "4. 日期格式: YYYY-MM-DD";
        ws2.Cell(6, 8).Value = "5. 部门名称需与系统一致（见左侧列表）";

        ws.Columns().AdjustToContents();
        ws2.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "资产导入模板.xlsx");
    }

    /// <summary>
    /// 导出全部资产为 Excel
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var assets = await _db.Assets.Include(a => a.Department).Include(a => a.AssignedUser).OrderBy(a => a.AssetTag).ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("资产清单");

        var headers = new[] { "资产编号", "名称", "分类", "品牌", "型号", "序列号", "状态", "采购日期", "采购价格", "过保日期", "位置", "部门", "使用人", "IP地址", "MAC地址", "备注" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        for (int r = 0; r < assets.Count; r++)
        {
            var a = assets[r];
            ws.Cell(r + 2, 1).Value = a.AssetTag;
            ws.Cell(r + 2, 2).Value = a.Name;
            ws.Cell(r + 2, 3).Value = CategoryLabel(a.Category);
            ws.Cell(r + 2, 4).Value = a.Brand ?? "";
            ws.Cell(r + 2, 5).Value = a.Model ?? "";
            ws.Cell(r + 2, 6).Value = a.SerialNumber ?? "";
            ws.Cell(r + 2, 7).Value = StatusLabel(a.Status);
            ws.Cell(r + 2, 8).Value = a.PurchaseDate?.ToString("yyyy-MM-dd") ?? "";
            ws.Cell(r + 2, 9).Value = a.PurchasePrice ?? 0;
            ws.Cell(r + 2, 9).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(r + 2, 10).Value = a.WarrantyExpiry?.ToString("yyyy-MM-dd") ?? "";
            ws.Cell(r + 2, 11).Value = a.Location ?? "";
            ws.Cell(r + 2, 12).Value = a.Department?.Name ?? "";
            ws.Cell(r + 2, 13).Value = a.AssignedUser?.Name ?? "";
            ws.Cell(r + 2, 14).Value = a.IpAddress ?? "";
            ws.Cell(r + 2, 15).Value = a.MacAddress ?? "";
            ws.Cell(r + 2, 16).Value = a.Notes ?? "";
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"资产清单_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    /// <summary>
    /// 批量导入资产
    /// </summary>
    [HttpPost("import")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "请选择文件" });

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "仅支持 .xlsx 格式" });

        // 缓存部门映射
        var departments = await _db.Departments.ToListAsync();
        var deptNameMap = departments.ToDictionary(d => d.Name, d => (string?)d.Id, StringComparer.OrdinalIgnoreCase);
        var deptCodeMap = departments.ToDictionary(d => d.Code, d => (string?)d.Id, StringComparer.OrdinalIgnoreCase);

        var existingTags = new HashSet<string>(await _db.Assets.Select(a => a.AssetTag).ToListAsync());

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.First();

        int success = 0, skip = 0, fail = 0;
        var errors = new List<string>();

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int r = 2; r <= lastRow; r++)
        {
            var tag = ws.Cell(r, 1).GetString().Trim();
            var name = ws.Cell(r, 2).GetString().Trim();

            if (string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(name))
            {
                skip++;
                continue;
            }

            if (existingTags.Contains(tag))
            {
                errors.Add($"第{r}行: 资产编号 {tag} 已存在");
                fail++;
                continue;
            }

            // 解析分类
            var catStr = ws.Cell(r, 3).GetString().Trim();
            if (!CategoryMap.TryGetValue(catStr, out var category))
                category = AssetCategory.Other;

            // 解析状态
            var statusStr = ws.Cell(r, 7).GetString().Trim();
            if (!StatusMap.TryGetValue(statusStr, out var status))
                status = AssetStatus.InStock;

            // 解析日期
            DateTime? purchaseDate = ParseDate(ws.Cell(r, 8));
            DateTime? warrantyExpiry = ParseDate(ws.Cell(r, 10));

            // 解析价格
            decimal? price = null;
            var priceCell = ws.Cell(r, 9);
            if (!priceCell.IsEmpty())
            {
                try { price = priceCell.GetValue<decimal>(); }
                catch
                {
                    try { price = decimal.Parse(priceCell.GetString().Trim().Replace(",", "").Replace("¥", "")); }
                    catch { }
                }
            }

            // 查找部门 ID
            var deptStr = ws.Cell(r, 12).GetString().Trim();
            string? deptId = null;
            if (!string.IsNullOrEmpty(deptStr))
            {
                if (!deptNameMap.TryGetValue(deptStr, out deptId))
                    deptCodeMap.TryGetValue(deptStr, out deptId);
            }

            var asset = new Asset
            {
                AssetTag = tag,
                Name = name,
                Category = category,
                Brand = ws.Cell(r, 4).GetString().Trim(),
                Model = ws.Cell(r, 5).GetString().Trim(),
                SerialNumber = ws.Cell(r, 6).GetString().Trim(),
                Status = status,
                PurchaseDate = purchaseDate,
                PurchasePrice = price,
                WarrantyExpiry = warrantyExpiry,
                Location = ws.Cell(r, 11).GetString().Trim(),
                DepartmentId = deptId,
                Notes = ws.Cell(r, 13).GetString().Trim(),
            };

            _db.Assets.Add(asset);
            existingTags.Add(tag);
            success++;
        }

        if (success > 0)
            await _db.SaveChangesAsync();

        return Ok(new { success, skip, fail, errors });
    }

    private static DateTime? ParseDate(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;
        try
        {
            if (cell.DataType == XLDataType.DateTime)
                return cell.GetDateTime();
            var str = cell.GetString().Trim();
            if (string.IsNullOrEmpty(str)) return null;
            if (DateTime.TryParse(str, out var dt)) return dt;
        }
        catch { }
        return null;
    }

    private static string CategoryLabel(AssetCategory c) => c switch
    {
        AssetCategory.Computer => "台式电脑",
        AssetCategory.Laptop => "笔记本",
        AssetCategory.Server => "服务器",
        AssetCategory.NetworkDevice => "网络设备",
        AssetCategory.Printer => "打印机",
        AssetCategory.Phone => "手机",
        AssetCategory.Software => "软件",
        AssetCategory.Monitor => "显示器",
        AssetCategory.Peripheral => "外设",
        _ => "其他",
    };

    private static string StatusLabel(AssetStatus s) => s switch
    {
        AssetStatus.InStock => "在库",
        AssetStatus.InUse => "使用中",
        AssetStatus.Maintenance => "维修中",
        AssetStatus.Retired => "已报废",
        AssetStatus.Disposed => "已处置",
        _ => "未知",
    };
}
