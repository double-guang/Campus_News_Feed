# 校园新闻推送系统

校园新闻推送系统是一个基于ASP.NET Core MVC的综合性新闻发布与管理平台，专为校园环境设计，提供个性化新闻推荐、分类浏览、搜索筛选等功能。系统采用莫兰迪色系的UI设计，美观且易于使用。

## 主要功能

### 用户功能
- **个性化新闻推荐**：基于用户偏好设置的智能推荐算法
- **分类浏览**：按不同新闻分类查看相关内容
- **搜索与筛选**：支持关键词搜索、分类筛选、日期范围筛选
- **排序选项**：支持综合排序、最新发布、最早发布等多种排序方式
- **用户偏好设置**：用户可设置感兴趣的新闻分类，获取个性化内容

### 管理功能
- **新闻管理**：添加、编辑、删除新闻内容
- **分类管理**：创建和管理新闻分类
- **用户管理**：查看用户信息、管理用户状态
- **数据分析**：查看新闻点击量、分类热度等统计信息

## 技术架构

- **后端框架**：ASP.NET Core MVC (.NET 9.0)
- **数据库**：MySQL (通过Entity Framework Core访问)
- **前端技术**：
  - Bootstrap 5 - 响应式UI框架
  - jQuery - JavaScript库
  - Bootstrap Icons - 图标库
- **主要设计模式**：
  - MVC架构模式
  - 仓储模式
  - 依赖注入
  - 服务层模式

## 系统架构

```
校园新闻推送系统
├── Controllers/          # 控制器层，处理请求和响应
├── Models/               # 数据模型层
│   ├── Domain/           # 领域模型
│   └── ViewModels/       # 视图模型
├── Views/                # 视图层，负责UI展示
├── Services/             # 服务层，处理业务逻辑
├── Data/                 # 数据访问层
├── Migrations/           # 数据库迁移文件
└── wwwroot/              # 静态资源文件
    ├── css/              # 样式文件
    ├── js/               # JavaScript文件
    └── images/           # 图片资源
```

## 特色功能详解

### 新闻推荐算法

系统使用基于多因素加权的推荐算法，综合考虑：
- 用户偏好的分类匹配度
- 新闻时效性（使用指数衰减函数）
- 新闻热度（点击量）
- 特殊时段加权（近期发布的新闻获得额外加分）

### 搜索与筛选系统

- **全文检索**：针对标题和内容的关键词搜索
- **多维度筛选**：支持分类、日期范围的组合筛选
- **智能排序**：根据不同条件提供适合的排序方式

### 用户体验优化

- **响应式设计**：适配不同尺寸的设备
- **莫兰迪色系UI**：舒适的视觉体验
- **渐进式交互**：操作简单直观
- **分页优化**：大数据量下的高效浏览体验

## 安装与部署

### 系统要求
- .NET 9.0 SDK
- MySQL 8.0+
- Visual Studio 2022+ 或 VS Code

### 数据库配置
1. 创建MySQL数据库：
```sql
CREATE DATABASE campus_news CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

2. 在`appsettings.json`中配置连接字符串：
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=campus_news;User=your_user;Password=your_password;"
}
```

3. 应用数据库迁移：
```bash
dotnet ef database update
```

### 启动项目
```bash
dotnet run
```

## 默认账户

系统初始化时会创建一个默认管理员账户：
- 邮箱: 
- 密码: 在首次运行时需要设置

## 开发与扩展

### 添加新功能
1. 在相应的Model文件夹中添加模型类
2. 在Services中实现业务逻辑
3. 在Controllers中添加控制器方法
4. 创建或修改Views中的视图文件

### 样式定制
系统使用了自定义的莫兰迪色系变量，可在`site.css`中进行修改：
```css
:root {
  --morandigreen-100: #e8efe8;
  --morandigreen-200: #d1e0d3;
  /* 更多颜色变量... */
}
```

## 代码规范

项目遵循以下代码规范：
- C# 代码风格遵循Microsoft推荐的.NET编码约定
- 视图文件使用Pascal命名法
- 服务接口以"I"开头
- 使用视图模型传递数据，避免使用ViewBag/ViewData

## 致谢

感谢所有为本项目做出贡献的开发者，以及使用到的开源项目：
- ASP.NET Core
- Entity Framework Core
- Bootstrap
- jQuery

## 许可证

本项目采用[MIT许可证](LICENSE)。 