using ClosedXML.Excel;
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
        {"笔记本电脑", AssetCategory.Laptop}, {"笔记本", AssetCategory.Laptop},
        {"台式机电脑", AssetCategory.Computer}, {"台式电脑", AssetCategory.Computer}, {"台式机", AssetCategory.Computer},
        {"服务器", AssetCategory.Server},
        {"网络安全", AssetCategory.NetworkDevice}, {"数通", AssetCategory.NetworkDevice},
        {"打印机", AssetCategory.Printer},
        {"扫描仪", AssetCategory.Peripheral},
        {"会议平板", AssetCategory.Monitor},
        {"显示器", AssetCategory.Monitor},
        {"鼠标", AssetCategory.Peripheral},
        {"键盘", AssetCategory.Peripheral},
        {"充电器", AssetCategory.Peripheral},
        {"拾音器", AssetCategory.Peripheral},
        {"扩展坞", AssetCategory.Peripheral},
        {"存储", AssetCategory.Server},
    };

    private static readonly Dictionary<string, AssetStatus> StatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        {"在库", AssetStatus.InStock},
        {"使用中", AssetStatus.InUse},
        {"已报废(资产)", AssetStatus.Retired},
        {"已报废(设备)", AssetStatus.Retired},
    };

    // 标准列标题映射（支持中英文）
    private const int COL_SN = 1;       // B列: sn/产品SN
    private const int COL_SAPSN = 2;    // C列: sapSn/SAP资产编码
    private const int COL_TYPE1 = 3;    // D列: assetType1/资产一级分类
    private const int COL_TYPE2 = 4;    // E列: assetType2/资产二级分类
    private const int COL_TYPE3 = 5;    // F列: assetType3/资产三级分类
    private const int COL_BRAND = 6;    // G列: brand/品牌
    private const int COL_MODEL = 7;    // H列: productModel/产品型号
    private const int COL_SPEC = 8;     // I列: specification/规格
    private const int COL_DATE = 9;     // J列: dateOfBuy/购买日期
    private const int COL_STATUS = 10;  // K列: assetStatus/资产状态
    private const int COL_USER = 11;    // L列: userName/使用人姓名
    private const int COL_DEPT_INI = 12;// M列: userDepartmentIni/使用人原部门
    private const int COL_DEPT_BACK = 13;// N列: userDepartmentBack/使用人归还部门
    private const int COL_COMP_INI = 14;// O列: userCompanyIni/使用人原公司
    private const int COL_COMP_BACK = 15;// P列: userCompanyBack/使用人归还公司
    private const int COL_BACK_DATE = 16;// Q列: backDate/归还日期
    private const int COL_APPLY_NAME = 17;// R列: applyName/领用人姓名
    private const int COL_APPLY_DATE = 18;// S列: applyDate/领用时间
    private const int COL_REMARK = 19;   // T列: remark/备注

    public AssetsImportController(AppDbContext db) { _db = db; }

    /// <summary>
    /// 下载导入模板（协鑫科技标准格式）
    /// </summary>
    [HttpGet("template")]
    public async Task<IActionResult> DownloadTemplate()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("管理中心资产管理台账");

        // 表头：Row 1 = 英文字段名，Row 2 = 中文说明
        var fields = new[] {
            ("id", "主键"),
            ("sn", "产品SN"),
            ("sapSn", "SAP资产编码"),
            ("assetType1", "资产一级分类"),
            ("assetType2", "资产二级分类"),
            ("assetType3", "资产三级分类"),
            ("brand", "品牌"),
            ("productModel", "产品型号"),
            ("specification", "规格"),
            ("dateOfBuy", "购买日期"),
            ("assetStatus", "资产状态"),
            ("userName", "使用人姓名"),
            ("userDepartmentIni", "使用人原部门"),
            ("userDepartmentBack", "使用人归还部门"),
            ("userCompanyIni", "使用人原公司"),
            ("userCompanyBack", "使用人归还公司"),
            ("backDate", "归还日期"),
            ("applyName", "领用人姓名"),
            ("applyDate", "领用时间"),
            ("remark", "备注"),
        };

        for (int i = 0; i < fields.Length; i++)
        {
            ws.Cell(1, i + 1).Value = fields[i].Item1;
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;

            ws.Cell(2, i + 1).Value = fields[i].Item2;
            ws.Cell(2, i + 1).Style.Font.Bold = true;
            ws.Cell(2, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // 示例数据
        ws.Cell(3, 1).Value = "";  // id
        ws.Cell(3, 2).Value = "SN202604001";
        ws.Cell(3, 3).Value = "";  // sapSn
        ws.Cell(3, 4).Value = "终端类";
        ws.Cell(3, 5).Value = "笔记本电脑";
        ws.Cell(3, 6).Value = "";  // assetType3
        ws.Cell(3, 7).Value = "惠普";
        ws.Cell(3, 8).Value = "630 G11";
        ws.Cell(3, 9).Value = "Intel Ultra 5&500G&16G";
        ws.Cell(3, 10).Value = "2025/12/24";
        ws.Cell(3, 11).Value = "使用中";
        ws.Cell(3, 12).Value = "张三";
        ws.Cell(3, 13).Value = "数字与信息化中心";
        ws.Cell(3, 14).Value = "";
        ws.Cell(3, 15).Value = "协鑫科技控股有限公司";
        ws.Cell(3, 16).Value = "";
        ws.Cell(3, 17).Value = "";
        ws.Cell(3, 18).Value = "张三";
        ws.Cell(3, 19).Value = "2025/12/24";
        ws.Cell(3, 20).Value = "";

        // 说明 sheet
        var ws2 = wb.Worksheets.Add("_说明");
        ws2.Cell(1, 1).Value = "一级分类";
        ws2.Cell(1, 1).Style.Font.Bold = true;
        var t1 = new[] { "终端类", "服务器类", "网络类", "备品备件类" };
        for (int i = 0; i < t1.Length; i++) ws2.Cell(i + 2, 1).Value = t1[i];

        ws2.Cell(1, 2).Value = "二级分类";
        ws2.Cell(1, 2).Style.Font.Bold = true;
        var t2 = new[] { "网络安全", "数通", "服务器", "存储", "笔记本电脑", "台式机电脑", "打印机", "扫描仪", "会议平板", "显示器", "鼠标", "键盘", "充电器", "拾音器", "扩展坞" };
        for (int i = 0; i < t2.Length; i++) ws2.Cell(i + 2, 2).Value = t2[i];

        ws2.Cell(1, 3).Value = "资产状态";
        ws2.Cell(1, 3).Style.Font.Bold = true;
        var st = new[] { "在库", "使用中", "已报废(资产)", "已报废(设备)" };
        for (int i = 0; i < st.Length; i++) ws2.Cell(i + 2, 3).Value = st[i];

        var depts = await _db.Departments.ToListAsync();
        ws2.Cell(1, 4).Value = "部门名称（系统中）";
        ws2.Cell(1, 4).Style.Font.Bold = true;
        for (int i = 0; i < depts.Count; i++)
        {
            ws2.Cell(i + 2, 4).Value = depts[i].Name;
            ws2.Cell(i + 2, 5).Value = depts[i].Id;
        }

        ws.Columns().AdjustToContents();
        ws2.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "资产台账导入模板.xlsx");
    }

    /// <summary>
    /// 导出全部资产（协鑫科技标准格式）
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var assets = await _db.Assets.Include(a => a.Department).Include(a => a.AssignedUser).OrderBy(a => a.AssetTag).ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("管理中心资产管理台账");

        var headers = new[] { "id", "sn", "sapSn", "assetType1", "assetType2", "assetType3", "brand", "productModel", "specification", "dateOfBuy", "assetStatus", "userName", "userDepartmentIni", "userDepartmentBack", "userCompanyIni", "userCompanyBack", "backDate", "applyName", "applyDate", "remark" };
        var labels = new[] { "主键", "产品SN", "SAP资产编码", "资产一级分类", "资产二级分类", "资产三级分类", "品牌", "产品型号", "规格", "购买日期", "资产状态", "使用人姓名", "使用人原部门", "使用人归还部门", "使用人原公司", "使用人归还公司", "归还日期", "领用人姓名", "领用时间", "备注" };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;

            ws.Cell(2, i + 1).Value = labels[i];
            ws.Cell(2, i + 1).Style.Font.Bold = true;
            ws.Cell(2, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        for (int r = 0; r < assets.Count; r++)
        {
            var a = assets[r];
            int row = r + 3;
            ws.Cell(row, 1).Value = a.Id;  // id
            ws.Cell(row, 2).Value = a.AssetTag;  // sn
            ws.Cell(row, 3).Value = a.SerialNumber ?? "";  // sapSn (复用 serialNumber)
            ws.Cell(row, 4).Value = Type1Label(a.Category);  // assetType1
            ws.Cell(row, 5).Value = Type2Label(a.Category);  // assetType2
            ws.Cell(row, 6).Value = "";  // assetType3
            ws.Cell(row, 7).Value = a.Brand ?? "";  // brand
            ws.Cell(row, 8).Value = a.Model ?? "";  // productModel
            ws.Cell(row, 9).Value = a.Description ?? "";  // specification
            ws.Cell(row, 10).Value = a.PurchaseDate?.ToString("yyyy/MM/dd") ?? "";  // dateOfBuy
            ws.Cell(row, 11).Value = StatusLabel(a.Status);  // assetStatus
            ws.Cell(row, 12).Value = a.AssignedUser?.Name ?? "";  // userName
            ws.Cell(row, 13).Value = a.Department?.Name ?? "";  // userDepartmentIni
            ws.Cell(row, 14).Value = "";  // userDepartmentBack
            ws.Cell(row, 15).Value = "协鑫科技控股有限公司";  // userCompanyIni
            ws.Cell(row, 16).Value = "";  // userCompanyBack
            ws.Cell(row, 17).Value = "";  // backDate
            ws.Cell(row, 18).Value = a.AssignedUser?.Name ?? "";  // applyName
            ws.Cell(row, 19).Value = a.PurchaseDate?.ToString("yyyy/MM/dd") ?? "";  // applyDate
            ws.Cell(row, 20).Value = a.Notes ?? "";  // remark
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"资产台账_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    /// <summary>
    /// 批量导入资产（协鑫科技标准格式）
    /// </summary>
    [HttpPost("import")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "请选择文件" });

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.First();

        // 自动检测列位置（支持 Row 1=英文头 或 Row 2=中文头）
        var colMap = DetectColumns(ws);
        if (colMap == null)
            return BadRequest(new { error = "无法识别表头，请使用标准模板格式" });

        // 缓存部门/用户
        var departments = await _db.Departments.ToListAsync();
        var users = await _db.Users.ToListAsync();
        var existingTags = new HashSet<string>(await _db.Assets.Select(a => a.AssetTag).ToListAsync());

        int success = 0, skip = 0, fail = 0;
        var errors = new List<string>();

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        int startRow = colMap.StartRow;

        for (int r = startRow; r <= lastRow; r++)
        {
            var sn = GetCell(ws, r, colMap.Sn).Trim();
            if (string.IsNullOrEmpty(sn))
            {
                skip++;
                continue;
            }

            if (existingTags.Contains(sn))
            {
                errors.Add($"第{r}行: 产品SN {sn} 已存在");
                fail++;
                continue;
            }

            var type1 = GetCell(ws, r, colMap.Type1);
            var type2 = GetCell(ws, r, colMap.Type2);
            var statusStr = GetCell(ws, r, colMap.Status);
            var deptStr = GetCell(ws, r, colMap.DeptIni);
            var userName = GetCell(ws, r, colMap.UserName);

            // 分类映射
            AssetCategory category = AssetCategory.Other;
            if (!string.IsNullOrEmpty(type2) && CategoryMap.TryGetValue(type2, out var cat))
                category = cat;

            // 状态映射
            if (!StatusMap.TryGetValue(statusStr, out var status))
                status = AssetStatus.InStock;

            // 日期解析
            DateTime? buyDate = ParseDate(ws, r, colMap.DateOfBuy);

            // 部门查找或自动创建
            string? deptId = null;
            if (!string.IsNullOrEmpty(deptStr))
            {
                var dept = departments.FirstOrDefault(d => d.Name == deptStr);
                if (dept != null)
                    deptId = dept.Id;
                else
                {
                    // 自动创建新部门
                    var newDept = new Department
                    {
                        Id = $"dept-{Guid.NewGuid():N}",
                        Name = deptStr,
                        Code = deptStr.Length > 10 ? deptStr[..10] : deptStr,
                    };
                    _db.Departments.Add(newDept);
                    departments.Add(newDept);
                    deptId = newDept.Id;
                }
            }

            // 使用人查找或自动创建
            string? userId = null;
            if (!string.IsNullOrEmpty(userName))
            {
                var user = users.FirstOrDefault(u => u.Name == userName);
                if (user != null)
                    userId = user.Id;
                else
                {
                    var newUser = new User
                    {
                        Id = $"user-{Guid.NewGuid():N}",
                        Name = userName,
                        EmployeeNo = userName,
                        DepartmentId = deptId,
                    };
                    _db.Users.Add(newUser);
                    users.Add(newUser);
                    userId = newUser.Id;
                }
            }

            var asset = new Asset
            {
                AssetTag = sn,
                Name = BuildAssetName(GetCell(ws, r, colMap.Brand), type2, GetCell(ws, r, colMap.Model)),
                Category = category,
                Brand = GetCell(ws, r, colMap.Brand),
                Model = GetCell(ws, r, colMap.Model),
                Description = GetCell(ws, r, colMap.Specification),
                SerialNumber = GetCell(ws, r, colMap.SapSn),
                Status = status,
                PurchaseDate = buyDate,
                DepartmentId = deptId,
                AssignedUserId = userId,
                Notes = GetCell(ws, r, colMap.Remark),
            };

            _db.Assets.Add(asset);
            existingTags.Add(sn);
            success++;
        }

        if (success > 0)
            await _db.SaveChangesAsync();

        return Ok(new { success, skip, fail, errors });
    }

    // ========== 辅助方法 ==========

    private record ColumnMap(int Sn, int SapSn, int Type1, int Type2, int Type3, int Brand, int Model, int Specification, int DateOfBuy, int Status, int UserName, int DeptIni, int Remark, int StartRow);

    private static ColumnMap? DetectColumns(IXLWorksheet ws)
    {
        // 检查 Row 1 是否是英文字段名
        var r1c2 = ws.Cell(1, 2).GetString().Trim().ToLower();
        if (r1c2 == "sn")
            return new ColumnMap(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 20, 3); // Row 1=英文, 数据从 Row 3 开始

        // 检查 Row 1 是否是中文字段名
        var r1c2cn = ws.Cell(1, 2).GetString().Trim();
        if (r1c2cn == "产品SN" || r1c2cn == "sn")
            return new ColumnMap(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 20, 2);

        return null;
    }

    private static string GetCell(IXLWorksheet ws, int row, int col)
    {
        return ws.Cell(row, col).GetString().Trim();
    }

    private static DateTime? ParseDate(IXLWorksheet ws, int row, int col)
    {
        var cell = ws.Cell(row, col);
        if (cell.IsEmpty()) return null;
        try
        {
            if (cell.DataType == XLDataType.DateTime)
                return cell.GetDateTime();
            if (cell.DataType == XLDataType.Number)
            {
                // Excel 数字日期（如 46065 = 2025-12-24）
                var serial = cell.GetDouble();
                return DateTime.FromOADate(serial);
            }
            var str = cell.GetString().Trim();
            if (string.IsNullOrEmpty(str)) return null;
            if (DateTime.TryParse(str, out var dt)) return dt;
            // 尝试 yyyy/M/d 格式
            if (DateTime.TryParseExact(str, new[] { "yyyy/M/d", "yyyy/MM/dd", "yyyy-MM-dd" },
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                return dt;
        }
        catch { }
        return null;
    }

    private static string BuildAssetName(string brand, string type2, string model)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(brand)) parts.Add(brand);
        if (!string.IsNullOrEmpty(type2)) parts.Add(type2);
        if (!string.IsNullOrEmpty(model)) parts.Add(model);
        return parts.Count > 0 ? string.Join(" ", parts) : "未命名资产";
    }

    private static string Type1Label(AssetCategory c) => c switch
    {
        AssetCategory.Computer or AssetCategory.Laptop or AssetCategory.Monitor or AssetCategory.Peripheral => "终端类",
        AssetCategory.Server or AssetCategory.Software => "服务器类",
        AssetCategory.NetworkDevice => "网络类",
        _ => "备品备件类",
    };

    private static string Type2Label(AssetCategory c) => c switch
    {
        AssetCategory.Laptop => "笔记本电脑",
        AssetCategory.Computer => "台式机电脑",
        AssetCategory.Server => "服务器",
        AssetCategory.NetworkDevice => "数通",
        AssetCategory.Printer => "打印机",
        AssetCategory.Monitor => "显示器",
        AssetCategory.Peripheral => "外设",
        _ => "其他",
    };

    private static string StatusLabel(AssetStatus s) => s switch
    {
        AssetStatus.InStock => "在库",
        AssetStatus.InUse => "使用中",
        AssetStatus.Maintenance => "使用中",
        AssetStatus.Retired => "已报废(资产)",
        AssetStatus.Disposed => "已报废(设备)",
        _ => "在库",
    };
}
