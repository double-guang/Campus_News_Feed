using Campus_News_Feed.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Campus_News_Feed.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<Token> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 检查数据库提供程序
            var isMySql = Database.ProviderName?.Contains("MySql") ?? false;

            // 配置表名
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<News>().ToTable("News");
            modelBuilder.Entity<UserPreference>().ToTable("UserPreferences");
            modelBuilder.Entity<Token>().ToTable("Tokens");
            
            // 配置Token与User的关系，避免级联删除
            modelBuilder.Entity<Token>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // 仅为MySQL配置字符集
            if (isMySql)
            {
                modelBuilder.Entity<User>().HasCharSet("utf8mb4");
                modelBuilder.Entity<Category>().HasCharSet("utf8mb4");
                modelBuilder.Entity<News>().HasCharSet("utf8mb4");
                modelBuilder.Entity<UserPreference>().HasCharSet("utf8mb4");
                modelBuilder.Entity<Token>().HasCharSet("utf8mb4");
            }

            // 种子数据 - 添加默认管理员
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "2964959746@qq.com",
                    Username = "admin",
                    PasswordHash = "000000", // 临时密码，将在首次启动时更新为正确哈希
                    UsePasswordLogin = true,
                    RegisteredAt = DateTime.UtcNow,
                    IsAdmin = true
                }
            );

            // 种子数据 - 添加默认新闻分类
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "校园动态", Description = "学校的最新动态和公告" },
                new Category { Id = 2, Name = "学术资讯", Description = "学术讲座、研讨会等相关信息" },
                new Category { Id = 3, Name = "体育赛事", Description = "校内外体育比赛和活动" },
                new Category { Id = 4, Name = "文化活动", Description = "艺术展览、文化节等活动" },
                new Category { Id = 5, Name = "就业指导", Description = "求职、实习和就业相关信息" }
            );
        }
    }
} 