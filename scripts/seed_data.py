#!/usr/bin/env python3
"""
ITAssetManager 初始化数据脚本
使用方法：
  1. 先启动后端：cd ITAssetManager.API && dotnet run --urls http://localhost:5099
  2. 运行本脚本：python3 scripts/seed_data.py
"""

import subprocess, json, sys

API = "http://localhost:5099"

def curl(method, path, data=None, token=None):
    cmd = ['curl', '-s', '-X', method, f'{API}{path}', '-H', 'Content-Type: application/json']
    if token:
        cmd += ['-H', f'Authorization: Bearer {token}']
    if data:
        cmd += ['-d', json.dumps(data)]
    r = subprocess.run(cmd, capture_output=True, text=True)
    try:
        return json.loads(r.stdout)
    except:
        return None

# Login
print("🔑 登录...")
resp = curl("POST", "/api/auth/login", {"username": "admin", "password": "admin123"})
if not resp or "token" not in resp:
    print("❌ 登录失败，请确认后端已启动")
    sys.exit(1)
token = resp["token"]
print("✅ 登录成功")

# 资产数据
assets = [
    # 笔记本 (8台)
    {"assetTag":"NB-001","name":"联想 ThinkPad X1 Carbon","category":1,"brand":"联想","model":"X1 Carbon Gen 11","status":1,"purchaseDate":"2025-06-15","purchasePrice":12999,"warrantyExpiry":"2028-06-15","location":"苏州总部3楼","departmentId":"dept-it"},
    {"assetTag":"NB-002","name":"联想 ThinkPad T14s","category":1,"brand":"联想","model":"T14s Gen 4","status":1,"purchaseDate":"2025-08-20","purchasePrice":8999,"warrantyExpiry":"2028-08-20","location":"苏州总部2楼","departmentId":"dept-fin"},
    {"assetTag":"NB-003","name":"戴尔 Latitude 5540","category":1,"brand":"戴尔","model":"Latitude 5540","status":1,"purchaseDate":"2025-03-10","purchasePrice":9599,"warrantyExpiry":"2028-03-10","location":"苏州总部2楼","departmentId":"dept-mkt"},
    {"assetTag":"NB-004","name":"苹果 MacBook Pro 14","category":1,"brand":"苹果","model":"MacBook Pro 14 M3","status":1,"purchaseDate":"2025-09-01","purchasePrice":16999,"warrantyExpiry":"2027-09-01","location":"苏州总部1楼","departmentId":"dept-hr"},
    {"assetTag":"NB-005","name":"华为 MateBook X Pro","category":1,"brand":"华为","model":"MateBook X Pro 2025","status":0,"purchaseDate":"2026-01-05","purchasePrice":11999,"warrantyExpiry":"2029-01-05","location":"苏州总部3楼","departmentId":"dept-it"},
    {"assetTag":"NB-006","name":"联想 ThinkPad E14","category":1,"brand":"联想","model":"E14 Gen 5","status":1,"purchaseDate":"2025-05-12","purchasePrice":5999,"warrantyExpiry":"2027-05-12","location":"上海分部","departmentId":"dept-mkt"},
    {"assetTag":"NB-007","name":"戴尔 XPS 13","category":1,"brand":"戴尔","model":"XPS 13 Plus","status":3,"purchaseDate":"2023-01-20","purchasePrice":10999,"warrantyExpiry":"2026-01-20","location":"仓库A","departmentId":"dept-it"},
    {"assetTag":"NB-008","name":"惠普 EliteBook 840","category":1,"brand":"惠普","model":"EliteBook 840 G10","status":1,"purchaseDate":"2025-11-08","purchasePrice":8599,"warrantyExpiry":"2028-11-08","location":"苏州总部2楼","departmentId":"dept-fin"},
    # 台式电脑 (6台)
    {"assetTag":"PC-001","name":"联想 ThinkCentre M920","category":0,"brand":"联想","model":"M920 Tower","status":1,"purchaseDate":"2024-06-10","purchasePrice":6999,"warrantyExpiry":"2027-06-10","location":"苏州总部3楼","departmentId":"dept-it"},
    {"assetTag":"PC-002","name":"戴尔 OptiPlex 7010","category":0,"brand":"戴尔","model":"OptiPlex 7010","status":1,"purchaseDate":"2024-09-15","purchasePrice":5499,"warrantyExpiry":"2027-09-15","location":"苏州总部2楼","departmentId":"dept-fin"},
    {"assetTag":"PC-003","name":"联想 ThinkCentre M720","category":0,"brand":"联想","model":"M720 SFF","status":1,"purchaseDate":"2024-03-20","purchasePrice":4999,"warrantyExpiry":"2027-03-20","location":"苏州总部1楼","departmentId":"dept-hr"},
    {"assetTag":"PC-004","name":"惠普 ProDesk 400 G9","category":0,"brand":"惠普","model":"ProDesk 400 G9","status":0,"purchaseDate":"2026-02-01","purchasePrice":5299,"warrantyExpiry":"2029-02-01","location":"仓库B","departmentId":"dept-it"},
    {"assetTag":"PC-005","name":"戴尔 OptiPlex 5090","category":0,"brand":"戴尔","model":"OptiPlex 5090","status":3,"purchaseDate":"2022-05-10","purchasePrice":4599,"warrantyExpiry":"2025-05-10","location":"仓库A","departmentId":"dept-it"},
    {"assetTag":"PC-006","name":"联想 ThinkCentre M920","category":0,"brand":"联想","model":"M920 Tiny","status":1,"purchaseDate":"2024-08-25","purchasePrice":5999,"warrantyExpiry":"2027-08-25","location":"上海分部","departmentId":"dept-mkt"},
    # 服务器 (4台)
    {"assetTag":"SRV-001","name":"戴尔 PowerEdge R750","category":2,"brand":"戴尔","model":"PowerEdge R750","status":1,"purchaseDate":"2024-01-15","purchasePrice":89999,"warrantyExpiry":"2029-01-15","location":"苏州机房","departmentId":"dept-it"},
    {"assetTag":"SRV-002","name":"联想 ThinkSystem SR650","category":2,"brand":"联想","model":"SR650 V3","status":1,"purchaseDate":"2024-06-20","purchasePrice":75999,"warrantyExpiry":"2029-06-20","location":"苏州机房","departmentId":"dept-it"},
    {"assetTag":"SRV-003","name":"华为 FusionServer 2288H","category":2,"brand":"华为","model":"2288H V6","status":1,"purchaseDate":"2025-03-10","purchasePrice":65999,"warrantyExpiry":"2030-03-10","location":"苏州机房","departmentId":"dept-it"},
    {"assetTag":"SRV-004","name":"戴尔 PowerEdge R640","category":2,"brand":"戴尔","model":"PowerEdge R640","status":2,"purchaseDate":"2022-08-15","purchasePrice":55999,"warrantyExpiry":"2025-08-15","location":"苏州机房","departmentId":"dept-it"},
    # 网络设备 (4台)
    {"assetTag":"NET-001","name":"华为 S5735-L24T4S","category":3,"brand":"华为","model":"S5735-L24T4S","status":1,"purchaseDate":"2024-04-10","purchasePrice":8999,"warrantyExpiry":"2029-04-10","location":"苏州总部3楼","departmentId":"dept-it"},
    {"assetTag":"NET-002","name":"思科 Catalyst 2960X","category":3,"brand":"思科","model":"WS-C2960X-48TS-L","status":1,"purchaseDate":"2023-07-20","purchasePrice":15999,"warrantyExpiry":"2026-07-20","location":"苏州总部2楼","departmentId":"dept-it"},
    {"assetTag":"NET-003","name":"华为 AR2240","category":3,"brand":"华为","model":"AR2240C","status":1,"purchaseDate":"2024-02-15","purchasePrice":25999,"warrantyExpiry":"2029-02-15","location":"苏州机房","departmentId":"dept-it"},
    {"assetTag":"NET-004","name":"H3C S5130S","category":3,"brand":"新华三","model":"S5130S-28S","status":0,"purchaseDate":"2026-01-20","purchasePrice":7999,"warrantyExpiry":"2031-01-20","location":"仓库B","departmentId":"dept-it"},
    # 打印机 (3台)
    {"assetTag":"PRT-001","name":"惠普 M404dn","category":4,"brand":"惠普","model":"M404dn","status":1,"purchaseDate":"2024-05-10","purchasePrice":2999,"warrantyExpiry":"2027-05-10","location":"苏州总部3楼","departmentId":"dept-it"},
    {"assetTag":"PRT-002","name":"佳能 iR 2625i","category":4,"brand":"佳能","model":"iR 2625i","status":1,"purchaseDate":"2024-08-20","purchasePrice":15999,"warrantyExpiry":"2027-08-20","location":"苏州总部1楼","departmentId":"dept-fin"},
    {"assetTag":"PRT-003","name":"惠普 M203dw","category":4,"brand":"惠普","model":"M203dw","status":3,"purchaseDate":"2022-03-15","purchasePrice":1599,"warrantyExpiry":"2025-03-15","location":"仓库A","departmentId":"dept-it"},
    # 显示器 (4台)
    {"assetTag":"MON-001","name":"戴尔 U2723QE 27寸","category":7,"brand":"戴尔","model":"U2723QE","status":1,"purchaseDate":"2025-06-01","purchasePrice":3299,"warrantyExpiry":"2028-06-01","location":"苏州总部3楼","departmentId":"dept-it"},
    {"assetTag":"MON-002","name":"戴尔 U2723QE 27寸","category":7,"brand":"戴尔","model":"U2723QE","status":1,"purchaseDate":"2025-06-01","purchasePrice":3299,"warrantyExpiry":"2028-06-01","location":"苏州总部2楼","departmentId":"dept-fin"},
    {"assetTag":"MON-003","name":"联想 T27p-30","category":7,"brand":"联想","model":"T27p-30","status":1,"purchaseDate":"2025-09-15","purchasePrice":3599,"warrantyExpiry":"2028-09-15","location":"苏州总部1楼","departmentId":"dept-hr"},
    {"assetTag":"MON-004","name":"LG 27UK850-W","category":7,"brand":"LG","model":"27UK850-W","status":0,"purchaseDate":"2026-02-01","purchasePrice":2999,"warrantyExpiry":"2029-02-01","location":"仓库B","departmentId":"dept-it"},
    # 电话 (3台)
    {"assetTag":"PHN-001","name":"iPhone 15 Pro","category":5,"brand":"苹果","model":"iPhone 15 Pro 256GB","status":1,"purchaseDate":"2025-09-20","purchasePrice":8999,"warrantyExpiry":"2026-09-20","location":"随身","departmentId":"dept-it"},
    {"assetTag":"PHN-002","name":"华为 Mate 60 Pro","category":5,"brand":"华为","model":"Mate 60 Pro 512GB","status":1,"purchaseDate":"2025-10-15","purchasePrice":7999,"warrantyExpiry":"2026-10-15","location":"随身","departmentId":"dept-mkt"},
    {"assetTag":"PHN-003","name":"iPhone 14","category":5,"brand":"苹果","model":"iPhone 14 128GB","status":3,"purchaseDate":"2023-09-10","purchasePrice":5999,"warrantyExpiry":"2024-09-10","location":"仓库A","departmentId":"dept-it"},
    # 外设 (3件)
    {"assetTag":"PER-001","name":"罗技 MX Keys","category":8,"brand":"罗技","model":"MX Keys","status":1,"purchaseDate":"2025-07-10","purchasePrice":699,"warrantyExpiry":"2027-07-10","location":"苏州总部3楼","departmentId":"dept-it"},
    {"assetTag":"PER-002","name":"罗技 MX Master 3S","category":8,"brand":"罗技","model":"MX Master 3S","status":1,"purchaseDate":"2025-07-10","purchasePrice":699,"warrantyExpiry":"2027-07-10","location":"苏州总部3楼","departmentId":"dept-it"},
    {"assetTag":"PER-003","name":"Jabra Evolve2 75","category":8,"brand":"Jabra","model":"Evolve2 75","status":1,"purchaseDate":"2025-08-01","purchasePrice":1999,"warrantyExpiry":"2027-08-01","location":"苏州总部2楼","departmentId":"dept-fin"},
]

# 插入资产
print(f"\n📦 导入 {len(assets)} 条资产...")
ok = 0
for a in assets:
    resp = curl("POST", "/api/assets", a, token)
    if resp and "id" in resp:
        ok += 1
    else:
        print(f"  ❌ {a['assetTag']}: {resp}")
print(f"✅ 资产导入完成: {ok}/{len(assets)}")

# 软件许可
licenses = [
    {"name":"Microsoft Office 365","version":"E3","totalLicenses":50,"vendor":"微软","category":"办公套件","expiryDate":"2027-03-31"},
    {"name":"Windows 11 Pro","version":"24H2","totalLicenses":40,"vendor":"微软","category":"操作系统"},
    {"name":"Adobe Creative Cloud","version":"2026","totalLicenses":10,"vendor":"Adobe","category":"设计","expiryDate":"2026-12-31"},
    {"name":"JetBrains All Products","version":"2025","totalLicenses":5,"vendor":"JetBrains","category":"开发工具","expiryDate":"2026-06-30"},
    {"name":"VMware vSphere","version":"8.0","totalLicenses":3,"vendor":"VMware","category":"虚拟化","expiryDate":"2025-12-31"},
]

print(f"\n📋 导入 {len(licenses)} 条软件许可...")
ok = 0
for lic in licenses:
    resp = curl("POST", "/api/softwarelicenses", lic, token)
    if resp and "id" in resp:
        ok += 1
print(f"✅ 许可导入完成: {ok}/{len(licenses)}")

print("\n🎉 初始化完成！")
print("   访问 http://localhost:5173 登录后体验 ChatBI 智能问数")
