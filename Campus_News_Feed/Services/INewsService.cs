using Campus_News_Feed.Models.Domain;

namespace Campus_News_Feed.Services
{
    public interface INewsService
    {
        Task<IEnumerable<News>> GetAllNewsAsync();
        Task<IEnumerable<News>> GetNewsByCategoryIdAsync(int categoryId);
        Task<IEnumerable<News>> GetRecommendedNewsForUserAsync(int userId);
        Task<News?> GetNewsByIdAsync(int id);
        Task<News> CreateNewsAsync(News news);
        Task<News?> UpdateNewsAsync(int id, News news);
        Task<bool> DeleteNewsAsync(int id);
        Task<bool> IncrementClickCountAsync(int id);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
    }
} 