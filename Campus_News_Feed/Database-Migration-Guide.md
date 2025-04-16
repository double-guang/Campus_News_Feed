# MySQL数据库迁移指南

本项目使用Entity Framework Core与MySQL数据库。以下是进行数据库迁移的步骤。

## 前提条件

1. 已安装MySQL服务器
2. 已安装.NET 9 SDK和EF Core工具
3. 已配置好appsettings.json中的数据库连接字符串

## 初始化数据库

首先，创建数据库：

```bash
mysql -u root -p < setup-database.sql
```

## 使用EF Core迁移工具

### 创建迁移

每次修改数据模型后，需要创建新的迁移：

```bash
dotnet ef migrations add [迁移名称]
```

例如：

```bash
dotnet ef migrations add InitialCreate
```

### 应用迁移

应用迁移到数据库：

```bash
dotnet ef database update
```

### 撤销迁移

如果需要撤销上一次迁移：

```bash
dotnet ef migrations remove
```

### 生成SQL脚本

生成SQL脚本而不直接应用迁移（用于生产环境）：

```bash
dotnet ef migrations script -o migration.sql
```

## 种子数据

系统默认会在数据库初始化时创建：
1. 默认管理员账号
2. 基本新闻分类

这些种子数据在`AppDbContext.cs`的`OnModelCreating`方法中配置。

## 注意事项

1. 在生产环境中应用迁移前，请务必备份数据库
2. 确保MySQL连接字符串中包含正确的凭据
3. 在团队开发中，所有成员应保持迁移同步 