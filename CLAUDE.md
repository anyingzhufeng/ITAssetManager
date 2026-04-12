# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# 后端
cd ITAssetManager.API
dotnet build                              # 构建
dotnet run --urls http://localhost:5099   # 运行
dotnet test                               # 运行测试

# 前端
cd frontend
npm install                               # 安装依赖
npm run dev                               # 开发模式
npm run build                             # 构建生产版本
```

## Architecture

- **Clean Architecture**: Core / Infrastructure / API / Tests
- **后端**: .NET 8 Web API + EF Core 8
- **前端**: Vue 3 + TypeScript + Element Plus + Vite
- **数据库**: SQLite（开发）/ PostgreSQL（生产），通过 `Database:Provider` 切换
- **认证**: JWT + 飞书 SSO（OAuth2）

### 项目结构

```
ITAssetManager/
├── ITAssetManager.Core/          # 实体、接口、枚举
│   ├── Entities/                 # Asset, Department, User, SoftwareLicense, AssetLog, SystemSetting
│   └── Interfaces/               # IRepositories
├── ITAssetManager.Infrastructure/ # EF Core、仓储实现、DI 配置
│   ├── Data/                     # AppDbContext + 种子数据
│   └── Repositories/             # 各仓储实现
├── ITAssetManager.API/           # Web API 层
│   ├── Controllers/              # 7 个控制器
│   ├── Services/                 # TokenService, FeishuService
│   └── Program.cs                # 入口 + 中间件配置
└── frontend/                     # Vue 3 前端
    └── src/
        ├── views/                # Login, Dashboard, Assets, Departments, Users, Licenses, Settings
        ├── api.ts                # Axios 封装 + 401 拦截器
        └── router.ts             # Vue Router + 路由守卫
```

## Conventions

- Controller 命名：`{Entity}Controller.cs`，复数名词
- API 路由：`/api/{entity}`，全部小写
- JSON 响应：camelCase（通过 `PropertyNamingPolicy = CamelCase`）
- 密码哈希：`SHA256(password + "ITAsset_Salt_2026")`，转 hex 小写
- 默认管理员：`admin` / `admin123`
- 端口：5099（开发环境）

## Database Config

通过 `appsettings.json` 切换：

```json
{
  "Database": {
    "Provider": "SQLite",              // 改为 "PostgreSQL" 切换
    "SqlitePath": "data/itasset.db",
    "Host": "localhost", "Port": "5432",
    "Name": "itasset", "User": "postgres", "Password": "postgres"
  }
}
```

## Key APIs

| Method | Endpoint | 说明 |
|--------|----------|------|
| POST | /api/auth/login | 用户名密码登录 |
| POST | /api/auth/feishu | 飞书 SSO |
| POST | /api/auth/change-password | 修改密码 |
| GET | /api/assets | 资产列表 |
| GET | /api/dashboard | 仪表盘统计 |
| GET | /api/settings | 系统设置 |
| PUT | /api/settings | 更新设置 |

## Do Not

- 不要用 `rm -f data/itasset.db`（会影响运行中的服务）
- 不要在 Controller 中直接操作数据库（通过 Repository）
- 不要用 Hungarian notation
- 不要硬编码密码哈希值（用 `AuthController.HashPassword()` 计算）
