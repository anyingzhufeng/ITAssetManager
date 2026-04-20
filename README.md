# ITAssetManager - IT 资产管理系统

基于 .NET 8 + Vue 3 的企业级 IT 资产管理平台。

## 技术栈

| 层 | 技术 |
|---|------|
| 后端 | .NET 8 Web API, EF Core 8, SQLite / PostgreSQL |
| 前端 | Vue 3, TypeScript, Element Plus, Vite |
| 认证 | JWT + 飞书 SSO (OAuth2) |
| 架构 | Clean Architecture (Core/Infrastructure/API) |

## 功能

- 💻 **IT 资产管理** — 10 种分类、5 种状态、全生命周期
- 🏢 **部门管理** — 树形组织架构
- 👤 **人员管理** — 工号、邮箱、飞书关联
- 📜 **软件许可** — 许可数量、到期跟踪
- 📊 **仪表盘** — 资产统计、状态分布
- ⚙️ **系统设置** — 飞书配置、系统参数
- 🔐 **JWT 认证** — 用户名密码 + 飞书 SSO

## 数据库

系统支持两种数据库，通过 `appsettings.json` 的 `Database:Provider` 切换：

### SQLite（开发/测试）
```json
{
  "Database": {
    "Provider": "SQLite",
    "SqlitePath": "data/itasset.db"
  }
}
```

### PostgreSQL（生产）
```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "Host": "localhost",
    "Port": "5432",
    "Name": "itasset",
    "User": "postgres",
    "Password": "your_password"
  }
}
```

## 快速开始

### 后端
```bash
cd ITAssetManager.API
dotnet run --urls http://localhost:5099
```

### 前端
```bash
cd frontend
npm install
npm run dev
```

### 默认账号
- 用户名: `admin`
- 密码: `admin123`

## API 文档

启动后访问 http://localhost:5099/swagger

## 版本

| 版本 | 日期 | 内容 |
|------|------|------|
| v0.1.0 | 2026-04-10 | 初始版本：基础 CRUD + JWT + 飞书配置 |
| v0.2.0 | 2026-04-10 | 切换 PostgreSQL 数据库 |
| v0.2.1 | 2026-04-10 | 支持 SQLite / PostgreSQL 双数据库切换 |
| v0.2.2 | 2026-04-12 | 添加 CLAUDE.md 项目指引文件 |
| v0.3.0 | 2026-04-20 | 新增 ChatBI 智能问数模块（自然语言查询 + ECharts 图表） |

## License

MIT
