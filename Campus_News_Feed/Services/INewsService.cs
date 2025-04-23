using Campus_News_Feed.Data;
using Campus_News_Feed.Models.Domain;
using Campus_News_Feed.Models.ViewModels;

namespace Campus_News_Feed.Services
{
    public interface INewsService
    {
        // 新闻管理 - 分页方法
        Task<PaginatedList<News>> GetPaginatedNewsAsync(int pageIndex, int pageSize);
        Task<PaginatedList<News>> GetPaginatedNewsByCategoryAsync(int categoryId, int pageIndex, int pageSize);
        Task<PaginatedList<News>> GetPaginatedRecommendedNewsAsync(int userId, int pageIndex, int pageSize);
        
        // 添加支持排序的方法
        Task<PaginatedList<News>> GetPaginatedNewsSortedAsync(int pageIndex, int pageSize, NewsSortOption sortOption, int? userId = null);
        Task<PaginatedList<News>> GetPaginatedNewsByCategorySortedAsync(int categoryId, int pageIndex, int pageSize, NewsSortOption sortOption, int? userId = null);
        
        // 添加支持搜索和筛选的方法
        Task<PaginatedList<News>> GetFilteredNewsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            int? categoryId = null, 
            DateTime? dateFrom = null, 
            DateTime? dateTo = null,
            NewsSortOption sortOption = NewsSortOption.Comprehensive);
        
        // 新闻管理 - 原有方法
        Task<IEnumerable<News>> GetAllNewsAsync();
        Task<News?> GetNewsByIdAsync(int id);
        Task<IEnumerable<News>> GetNewsByCategoryAsync(int categoryId);
        Task<IEnumerable<News>> GetRecommendedNewsAsync(int userId);
        Task<IEnumerable<News>> GetHotNewsAsync(int count);
        Task<News> CreateNewsAsync(News news);
        Task<bool> UpdateNewsAsync(News news);
        Task<bool> DeleteNewsAsync(int id);
        Task<bool> IncrementClickCountAsync(int newsId);
        
        // 分类管理
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<int> GetNewsByCategoryCountAsync(int categoryId);
        Task<Category> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
        
        /// <summary>
        /// 获取新闻总数
        /// </summary>
        Task<int> GetNewsCountAsync();
        
        // 数据库访问 - 用于特殊操作如生成测试数据
        AppDbContext GetDbContext();
    }
} 