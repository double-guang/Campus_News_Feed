using Campus_News_Feed.Models.Domain;

namespace Campus_News_Feed.Models.ViewModels
{
    // 排序方式枚举
    public enum NewsSortOption
    {
        Comprehensive, // 综合排序
        TimeDesc,      // 时间降序（新到旧）
        TimeAsc        // 时间升序（旧到新）
    }

    public class NewsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ClickCount { get; set; }

        public static NewsViewModel FromNews(News news)
        {
            return new NewsViewModel
            {
                Id = news.Id,
                Title = news.Title,
                Content = news.Content,
                CategoryId = news.CategoryId,
                CategoryName = news.Category?.Name ?? "未分类",
                CreatedAt = news.CreatedAt,
                UpdatedAt = news.UpdatedAt,
                ClickCount = news.ClickCount
            };
        }
    }

    public class NewsListViewModel
    {
        public List<NewsViewModel> News { get; set; } = new List<NewsViewModel>();
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        
        // 排序选项
        public NewsSortOption SortOption { get; set; } = NewsSortOption.Comprehensive;
        
        // 分页属性
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int TotalItems { get; set; }
        
        // 搜索和筛选属性
        public string? SearchTerm { get; set; }
        public int? FilterCategoryId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        
        // 帮助器方法 - 生成排序链接，保留当前筛选条件
        public string GetSortUrl(NewsSortOption sortOption)
        {
            var searchTermParam = string.IsNullOrEmpty(SearchTerm) ? 
                string.Empty : $"&searchTerm={System.Text.Encodings.Web.UrlEncoder.Default.Encode(SearchTerm)}";
            var categoryParam = FilterCategoryId.HasValue ? 
                $"&categoryId={FilterCategoryId.Value}" : string.Empty;
            var dateFromParam = DateFrom.HasValue ?
                $"&dateFrom={DateFrom.Value:yyyy-MM-dd}" : string.Empty;
            var dateToParam = DateTo.HasValue ?
                $"&dateTo={DateTo.Value:yyyy-MM-dd}" : string.Empty;
                
            return $"?page=1&sortOption={sortOption}{searchTermParam}{categoryParam}{dateFromParam}{dateToParam}";
        }
        
        // 帮助器方法 - 获取分页URL
        public string GetPageUrl(int pageNumber)
        {
            var searchTermParam = string.IsNullOrEmpty(SearchTerm) ? 
                string.Empty : $"&searchTerm={System.Text.Encodings.Web.UrlEncoder.Default.Encode(SearchTerm)}";
            var categoryParam = FilterCategoryId.HasValue ? 
                $"&categoryId={FilterCategoryId.Value}" : string.Empty;
            var sortParam = $"&sortOption={SortOption}";
            var dateFromParam = DateFrom.HasValue ?
                $"&dateFrom={DateFrom.Value:yyyy-MM-dd}" : string.Empty;
            var dateToParam = DateTo.HasValue ?
                $"&dateTo={DateTo.Value:yyyy-MM-dd}" : string.Empty;
                
            return $"?page={pageNumber}{sortParam}{searchTermParam}{categoryParam}{dateFromParam}{dateToParam}";
        }
        
        // 帮助器方法 - 获取类别筛选URL
        public string GetCategoryFilterUrl(int? categoryId)
        {
            var searchTermParam = string.IsNullOrEmpty(SearchTerm) ? 
                string.Empty : $"&searchTerm={System.Text.Encodings.Web.UrlEncoder.Default.Encode(SearchTerm)}";
            var categoryParam = categoryId.HasValue ? 
                $"&categoryId={categoryId.Value}" : string.Empty;
            var sortParam = $"&sortOption={SortOption}";
            var dateFromParam = DateFrom.HasValue ?
                $"&dateFrom={DateFrom.Value:yyyy-MM-dd}" : string.Empty;
            var dateToParam = DateTo.HasValue ?
                $"&dateTo={DateTo.Value:yyyy-MM-dd}" : string.Empty;
                
            return $"?page=1{sortParam}{searchTermParam}{categoryParam}{dateFromParam}{dateToParam}";
        }
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