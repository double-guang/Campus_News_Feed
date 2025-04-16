using Campus_News_Feed.Models.Domain;

namespace Campus_News_Feed.Models.ViewModels
{
    public class NewsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ClickCount { get; set; }

        public static NewsViewModel FromNews(News news)
        {
            return new NewsViewModel
            {
                Id = news.Id,
                Title = news.Title,
                Content = news.Content,
                CategoryId = news.CategoryId,
                CategoryName = news.Category?.Name ?? string.Empty,
                CreatedAt = news.CreatedAt,
                ClickCount = news.ClickCount
            };
        }
    }

    public class NewsListViewModel
    {
        public List<NewsViewModel> News { get; set; } = new List<NewsViewModel>();
        public string? CategoryName { get; set; }
        public bool IsRecommended { get; set; }
    }

    public class NewsCreateViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
    }

    public class NewsEditViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
    }
} 