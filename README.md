# 校园新闻推送系统

基于.NET9的简洁架构设计的校园新闻推送系统。

## 项目架构

### 技术栈
- 后端: .NET 9, ASP.NET Core MVC
- 数据库: Entity Framework Core + MySQL
- 前端: Razor Pages + Bootstrap

### 目录结构
```
Campus_News_Feed/
├── Controllers/        # 控制器目录
├── Models/             # 数据模型
│   ├── Domain/         # 领域模型
│   ├── DTOs/           # 数据传输对象
│   └── ViewModels/     # 视图模型
├── Views/              # 视图文件
├── Services/           # 业务逻辑服务
├── Data/               # 数据访问相关
│   └── Repositories/   # 仓储模式实现
├── Utilities/          # 工具类
├── wwwroot/            # 静态资源文件
└── Program.cs          # 应用程序入口
```

### 核心功能模块
1. **认证模块** - 基于邮箱令牌的无密码认证
2. **用户模块** - 用户信息管理和新闻偏好设置
3. **新闻模块** - 新闻内容管理和展示
4. **推荐模块** - 基于用户偏好的新闻推荐
5. **管理模块** - 管理员后台功能

### 数据模型
- User: 用户信息
- News: 新闻内容
- Category: 新闻分类
- UserPreference: 用户偏好设置
- Token: 认证令牌

## 功能说明

### 用户端功能

1. **注册与登录**
   - 基于邮箱验证的无密码登录
   - 支持学校邮箱域名验证
   - 通过邮件链接完成注册和登录

2. **个人信息管理**
   - 查看个人信息
   - 设置新闻偏好（选择感兴趣的新闻类别）

3. **新闻推荐**
   - 根据用户偏好自动推荐新闻
   - 基于历史点击率排序

4. **新闻分类查询**
   - 按分类浏览新闻
   - 新闻点击率统计

### 管理员端功能

1. **管理员登录**
   - 基于预设管理员账号

2. **新闻管理**
   - 添加新闻
   - 修改新闻
   - 删除新闻

## 如何启动

1. **准备开发环境**
   - 安装 .NET 9 SDK
   - 安装MySQL服务器
   - 确保已安装Entity Framework Core工具

2. **克隆代码库**
   ```bash
   git clone https://github.com/your-repo/Campus_News_Feed.git
   cd Campus_News_Feed
   ```

3. **设置数据库**
   - 创建MySQL数据库
   ```bash
   mysql -u root -p < setup-database.sql
   ```
   - 编辑 `appsettings.json` 中的连接字符串，设置正确的MySQL服务器信息

4. **配置应用**
   - 编辑 `appsettings.json` 配置文件
   - 配置邮件服务器设置
   - 配置学校邮箱域名

5. **运行应用**
   ```bash
   dotnet run
   ```

6. **访问应用**
   - 浏览器访问: https://localhost:7173

## 默认账号

系统初始化时会创建默认管理员账号:
- 邮箱: admin@school.edu
- 登录方式: 与普通用户相同，通过邮箱验证链接

### API设计
- 用户认证API
- 用户信息管理API
- 新闻内容API
- 新闻分类API
- 管理员API 
