using Campus_News_Feed.Data;
using Campus_News_Feed.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Campus_News_Feed.Services
{
    public class NewsService : INewsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NewsService> _logger;

        public NewsService(AppDbContext context, ILogger<NewsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<News>> GetAllNewsAsync()
        {
            return await _context.News
                .Include(n => n.Category)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<News>> GetNewsByCategoryIdAsync(int categoryId)
        {
            return await _context.News
                .Include(n => n.Category)
                .Where(n => n.CategoryId == categoryId)
                .OrderByDescending(n => n.ClickCount)
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<News>> GetRecommendedNewsForUserAsync(int userId)
        {
            // 获取用户偏好的新闻类别
            var userPreferredCategories = await _context.UserPreferences
                .Where(up => up.UserId == userId)
                .Select(up => up.CategoryId)
                .ToListAsync();

            if (!userPreferredCategories.Any())
            {
                // 如果用户没有设置偏好，返回所有分类的热门新闻
                return await _context.News
                    .Include(n => n.Category)
                    .OrderByDescending(n => n.ClickCount)
                    .ThenByDescending(n => n.CreatedAt)
                    .Take(10)
                    .ToListAsync();
            }

            // 根据用户偏好返回相关类别的新闻
            return await _context.News
                .Include(n => n.Category)
                .Where(n => userPreferredCategories.Contains(n.CategoryId))
                .OrderByDescending(n => n.ClickCount)
                .ThenByDescending(n => n.CreatedAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task<News?> GetNewsByIdAsync(int id)
        {
            return await _context.News
                .Include(n => n.Category)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<News> CreateNewsAsync(News news)
        {
            news.CreatedAt = DateTime.UtcNow;
            
            _context.News.Add(news);
            await _context.SaveChangesAsync();
            
            return news;
        }

        public async Task<News?> UpdateNewsAsync(int id, News updatedNews)
        {
            var existingNews = await _context.News.FindAsync(id);
            if (existingNews == null)
            {
                return null;
            }

            existingNews.Title = updatedNews.Title;
            existingNews.Content = updatedNews.Content;
            existingNews.CategoryId = updatedNews.CategoryId;
            existingNews.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingNews;
        }

        public async Task<bool> DeleteNewsAsync(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return false;
            }

            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementClickCountAsync(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return false;
            }

            news.ClickCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }
    }
} 