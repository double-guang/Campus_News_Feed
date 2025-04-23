using Campus_News_Feed.Data;
using Campus_News_Feed.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 添加服务到依赖注入容器
builder.Services.AddControllersWithViews();

// 配置日志 - 增加更详细的日志输出
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information); // 确保能看到信息级别的日志

// 配置数据库
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbProvider = builder.Configuration["Database:Provider"]?.ToLower() ?? "mysql";

if (dbProvider == "mysql")
{
    Console.WriteLine("使用MySQL数据库");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure()
        )
    );
}
else
{
    Console.WriteLine("使用SQLite数据库");
    // 使用SQLite
    var sqliteConnectionString = "Data Source=campus_news.db";
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(sqliteConnectionString)
    );
}

// 注册服务
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISessionService, SessionService>();

// 配置会话
builder.Services.AddDistributedMemoryCache(); // 使用内存缓存存储会话数据
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 会话过期时间
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 配置HTTP请求管道
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 启用会话
app.UseSession();

// 配置路由
app.MapControllerRoute(
    name: "auth",
    pattern: "auth/{action=Index}/{id?}",
    defaults: new { controller = "Auth" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// 确保数据库创建并应用迁移
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        var authService = services.GetRequiredService<IAuthService>();
        
        logger.LogInformation("检查数据库...");
        
        // 确保数据库存在，如果不存在则创建
        // 注意：这不会删除现有数据
        if (context.Database.EnsureCreated())
        {
            logger.LogInformation("数据库已创建并初始化");
            
            // 为管理员设置默认密码
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.IsAdmin && u.Id == 1);
            if (adminUser != null && adminUser.PasswordHash == "000000")
            {
                // 设置默认管理员密码为 "admin123"
                await authService.SetAdminPasswordAsync(adminUser.Id, "admin123");
                logger.LogInformation("已为管理员设置默认密码");
            }
        }
        else
        {
            logger.LogInformation("数据库已存在，无需创建");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "检查数据库时出错");
        throw; // 在开发环境抛出异常以便查看详细错误
    }
}

app.Run();
