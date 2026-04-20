using ITAssetManager.Core.Entities;
using ITAssetManager.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatBIController : ControllerBase
{
    private readonly AppDbContext _db;

    public ChatBIController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 对话式 BI 查询
    /// </summary>
    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] ChatBIRequest request)
    {
        var question = request.Question?.Trim() ?? "";
        if (string.IsNullOrEmpty(question))
            return BadRequest(new { error = "问题不能为空" });

        // 意图识别 + 数据查询
        var result = await MatchAndQuery(question);
        return Ok(result);
    }

    /// <summary>
    /// 获取可用的查询模板（帮助用户提问）
    /// </summary>
    [HttpGet("templates")]
    public IActionResult GetTemplates()
    {
        var templates = new[]
        {
            new { label = "各类资产数量分布", icon = "📊" },
            new { label = "资产状态统计", icon = "📋" },
            new { label = "各部门资产分布", icon = "🏢" },
            new { label = "月度新增资产趋势", icon = "📈" },
            new { label = "资产价值统计", icon = "💰" },
            new { label = "品牌分布", icon = "🏷️" },
            new { label = "存放位置统计", icon = "📍" },
            new { label = "资产总数", icon = "🔢" },
            new { label = "即将过保资产", icon = "⚠️" },
            new { label = "软件许可使用情况", icon = "💿" },
        };
        return Ok(templates);
    }

    private async Task<object> MatchAndQuery(string question)
    {
        // ===== 规则 1：各类资产数量分布（饼图）=====
        if (MatchAny(question, "各类", "分布", "分类", "类别", "数量"))
        {
            var data = await _db.Assets
                .GroupBy(a => a.Category)
                .Select(g => new { name = g.Key.ToString(), value = g.Count() })
                .ToListAsync();

            return new
            {
                answer = $"系统中共有 {data.Sum(d => d.value)} 件资产，按类别分布如下：\n" +
                         string.Join("\n", data.Select(d => $"- {CategoryName(d.name)}：{d.value} 件")),
                chartType = "pie",
                chartData = new
                {
                    title = "资产类别分布",
                    series = data.Select(d => new { name = CategoryName(d.name), value = d.value })
                },
                sql = "SELECT Category, COUNT(*) FROM Assets GROUP BY Category"
            };
        }

        // ===== 规则 2：资产状态统计（柱状图）=====
        if (MatchAny(question, "状态", "使用中", "闲置", "报废", "在库", "维修"))
        {
            var data = await _db.Assets
                .GroupBy(a => a.Status)
                .Select(g => new { name = g.Key.ToString(), value = g.Count() })
                .ToListAsync();

            return new
            {
                answer = $"资产状态分布：\n" +
                         string.Join("\n", data.Select(d => $"- {StatusName(d.name)}：{d.value} 件")),
                chartType = "bar",
                chartData = new
                {
                    title = "资产状态统计",
                    categories = data.Select(d => StatusName(d.name)),
                    values = data.Select(d => d.value)
                },
                sql = "SELECT Status, COUNT(*) FROM Assets GROUP BY Status"
            };
        }

        // ===== 规则 3：各部门资产分布（横向柱状图）=====
        if (MatchAny(question, "部门", "哪个部门"))
        {
            var data = await _db.Assets
                .Include(a => a.Department)
                .GroupBy(a => a.Department != null ? a.Department.Name : "未分配")
                .Select(g => new { name = g.Key, value = g.Count() })
                .OrderByDescending(d => d.value)
                .ToListAsync();

            return new
            {
                answer = $"各部门资产分布：\n" +
                         string.Join("\n", data.Select(d => $"- {d.name}：{d.value} 件")),
                chartType = "barHorizontal",
                chartData = new
                {
                    title = "各部门资产数量",
                    categories = data.Select(d => d.name),
                    values = data.Select(d => d.value)
                },
                sql = "SELECT d.Name, COUNT(*) FROM Assets a LEFT JOIN Departments d ON a.DepartmentId = d.Id GROUP BY d.Name"
            };
        }

        // ===== 规则 4：月度新增趋势（折线图）=====
        if (MatchAny(question, "趋势", "月度", "新增", "增长", "每月"))
        {
            var data = await _db.Assets
                .Where(a => a.CreatedAt != default)
                .GroupBy(a => new { a.CreatedAt.Year, a.CreatedAt.Month })
                .Select(g => new
                {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    count = g.Count()
                })
                .OrderBy(d => d.year).ThenBy(d => d.month)
                .ToListAsync();

            return new
            {
                answer = $"资产新增趋势（按月）：\n" +
                         string.Join("\n", data.Select(d => $"- {d.year}年{d.month}月：新增 {d.count} 件")),
                chartType = "line",
                chartData = new
                {
                    title = "月度新增资产趋势",
                    categories = data.Select(d => $"{d.year}-{d.month:D2}"),
                    values = data.Select(d => d.count)
                },
                sql = "SELECT strftime('%Y-%m', CreatedAt), COUNT(*) FROM Assets GROUP BY strftime('%Y-%m', CreatedAt)"
            };
        }

        // ===== 规则 5：资产价值统计（柱状图）=====
        if (MatchAny(question, "价值", "金额", "总值", "价格", "多少钱"))
        {
            var data = await _db.Assets
                .Where(a => a.PurchasePrice != null)
                .GroupBy(a => a.Category)
                .Select(g => new
                {
                    name = g.Key.ToString(),
                    value = g.Sum(a => a.PurchasePrice ?? 0),
                    count = g.Count()
                })
                .ToListAsync();

            return new
            {
                answer = $"资产价值统计（按类别）：\n" +
                         string.Join("\n", data.Select(d => $"- {CategoryName(d.name)}：{d.count} 件，总值 ¥{d.value:N2}")),
                chartType = "bar",
                chartData = new
                {
                    title = "各类资产总价值（元）",
                    categories = data.Select(d => CategoryName(d.name)),
                    values = data.Select(d => d.value)
                },
                sql = "SELECT Category, SUM(PurchasePrice) FROM Assets GROUP BY Category"
            };
        }

        // ===== 规则 6：品牌分布（饼图）=====
        if (MatchAny(question, "品牌", "厂商", "制造商"))
        {
            var data = await _db.Assets
                .Where(a => a.Brand != null && a.Brand != "")
                .GroupBy(a => a.Brand!)
                .Select(g => new { name = g.Key, value = g.Count() })
                .OrderByDescending(d => d.value)
                .Take(10)
                .ToListAsync();

            return new
            {
                answer = $"品牌分布 TOP {data.Count}：\n" +
                         string.Join("\n", data.Select(d => $"- {d.name}：{d.value} 件")),
                chartType = "pie",
                chartData = new
                {
                    title = "品牌分布 TOP 10",
                    series = data.Select(d => new { name = d.name, value = d.value })
                },
                sql = "SELECT Brand, COUNT(*) FROM Assets WHERE Brand IS NOT NULL GROUP BY Brand ORDER BY COUNT(*) DESC LIMIT 10"
            };
        }

        // ===== 规则 7：位置统计（柱状图）=====
        if (MatchAny(question, "位置", "仓库", "存放", "在哪", "地点"))
        {
            var data = await _db.Assets
                .Where(a => a.Location != null && a.Location != "")
                .GroupBy(a => a.Location!)
                .Select(g => new { name = g.Key, value = g.Count() })
                .OrderByDescending(d => d.value)
                .ToListAsync();

            return new
            {
                answer = $"资产存放位置分布：\n" +
                         string.Join("\n", data.Select(d => $"- {d.name}：{d.value} 件")),
                chartType = "bar",
                chartData = new
                {
                    title = "资产存放位置统计",
                    categories = data.Select(d => d.name),
                    values = data.Select(d => d.value)
                },
                sql = "SELECT Location, COUNT(*) FROM Assets WHERE Location IS NOT NULL GROUP BY Location"
            };
        }

        // ===== 规则 8：总数统计（数字卡片）=====
        if (MatchAny(question, "总数", "一共", "总共有", "多少件", "多少台", "多少个"))
        {
            var total = await _db.Assets.CountAsync();
            var inUse = await _db.Assets.CountAsync(a => a.Status == AssetStatus.InUse);
            var inStock = await _db.Assets.CountAsync(a => a.Status == AssetStatus.InStock);
            var deptCount = await _db.Departments.CountAsync();
            var userCount = await _db.Users.CountAsync();

            return new
            {
                answer = $"📊 资产总览：\n" +
                         $"- 资产总数：{total} 件\n" +
                         $"- 使用中：{inUse} 件\n" +
                         $"- 在库：{inStock} 件\n" +
                         $"- 部门数：{deptCount} 个\n" +
                         $"- 人员数：{userCount} 人",
                chartType = "card",
                chartData = new
                {
                    title = "资产总览",
                    cards = new[]
                    {
                        new { label = "资产总数", value = total.ToString(), color = "#409EFF" },
                        new { label = "使用中", value = inUse.ToString(), color = "#67C23A" },
                        new { label = "在库", value = inStock.ToString(), color = "#E6A23C" },
                        new { label = "部门数", value = deptCount.ToString(), color = "#909399" },
                    }
                },
                sql = "SELECT COUNT(*) FROM Assets"
            };
        }

        // ===== 规则 9：即将过保（表格）=====
        if (MatchAny(question, "过保", "保修", "到期", "即将过期"))
        {
            var data = await _db.Assets
                .Where(a => a.WarrantyExpiry != null && a.WarrantyExpiry <= DateTime.UtcNow.AddMonths(3))
                .OrderBy(a => a.WarrantyExpiry)
                .Select(a => new
                {
                    a.AssetTag,
                    a.Name,
                    a.Brand,
                    a.Model,
                    WarrantyExpiry = a.WarrantyExpiry!.Value.ToString("yyyy-MM-dd"),
                    DaysLeft = (a.WarrantyExpiry!.Value - DateTime.UtcNow).Days
                })
                .Take(20)
                .ToListAsync();

            return new
            {
                answer = data.Count == 0
                    ? "✅ 好消息！没有即将过保的资产（3个月内）。"
                    : $"⚠️ 以下 {data.Count} 件资产将在 3 个月内过保：\n" +
                      string.Join("\n", data.Select(d => $"- {d.AssetTag} {d.Name}（{d.Brand} {d.Model}）：{d.WarrantyExpiry}，剩余 {d.DaysLeft} 天")),
                chartType = "table",
                chartData = new
                {
                    title = "即将过保资产（3个月内）",
                    columns = new[] { "资产编号", "名称", "品牌", "型号", "过保日期", "剩余天数" },
                    rows = data.Select(d => new[] { d.AssetTag, d.Name, d.Brand ?? "", d.Model ?? "", d.WarrantyExpiry, d.DaysLeft.ToString() })
                },
                sql = "SELECT * FROM Assets WHERE WarrantyExpiry <= date('now', '+3 months') ORDER BY WarrantyExpiry"
            };
        }

        // ===== 规则 10：软件许可（环形图）=====
        if (MatchAny(question, "软件", "许可", "license", "授权"))
        {
            var data = await _db.SoftwareLicenses
                .Select(s => new
                {
                    s.Name,
                    s.TotalLicenses,
                    s.UsedLicenses,
                    Remaining = s.TotalLicenses - s.UsedLicenses,
                    UsageRate = s.TotalLicenses > 0 ? (double)s.UsedLicenses / s.TotalLicenses * 100 : 0
                })
                .ToListAsync();

            return new
            {
                answer = data.Count == 0
                    ? "系统中暂无软件许可记录。"
                    : $"软件许可使用情况：\n" +
                      string.Join("\n", data.Select(d =>
                          $"- {d.Name}：已用 {d.UsedLicenses}/{d.TotalLicenses}（使用率 {d.UsageRate:F1}%）")),
                chartType = "doughnut",
                chartData = new
                {
                    title = "软件许可使用率",
                    series = data.Select(d => new { name = d.Name, value = d.UsedLicenses, total = d.TotalLicenses })
                },
                sql = "SELECT Name, TotalLicenses, UsedLicenses FROM SoftwareLicenses"
            };
        }

        // ===== 未匹配 → 返回提示 =====
        return new
        {
            answer = "🤔 抱歉，我暂时无法理解这个问题。试试这些查询：\n" +
                     "- 各类资产数量分布\n" +
                     "- 资产状态统计\n" +
                     "- 各部门资产分布\n" +
                     "- 月度新增资产趋势\n" +
                     "- 资产价值统计\n" +
                     "- 品牌分布\n" +
                     "- 存放位置统计\n" +
                     "- 资产总数\n" +
                     "- 即将过保资产\n" +
                     "- 软件许可使用情况",
            chartType = "none",
            chartData = (object?)null,
            sql = ""
        };
    }

    private static bool MatchAny(string question, params string[] keywords)
    {
        return keywords.Any(k => question.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static string CategoryName(string name) => name switch
    {
        "Computer" => "台式电脑",
        "Laptop" => "笔记本",
        "Server" => "服务器",
        "NetworkDevice" => "网络设备",
        "Printer" => "打印机",
        "Phone" => "电话/手机",
        "Software" => "软件",
        "Monitor" => "显示器",
        "Peripheral" => "外设",
        "Other" => "其他",
        _ => name
    };

    private static string StatusName(string name) => name switch
    {
        "InStock" => "在库",
        "InUse" => "使用中",
        "Maintenance" => "维修中",
        "Retired" => "已报废",
        "Disposed" => "已处置",
        _ => name
    };
}

public class ChatBIRequest
{
    public string? Question { get; set; }
}
