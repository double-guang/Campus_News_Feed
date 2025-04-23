using Campus_News_Feed.Models.Domain;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;

namespace Campus_News_Feed.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsAdmin { get; set; }
        public List<int> SelectedCategories { get; set; } = new List<int>();
        public List<Category> AllCategories { get; set; } = new List<Category>();
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; } = true;

        public static UserViewModel FromUser(User user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username ?? string.Empty,
                RegisteredAt = user.RegisteredAt,
                LastLoginAt = user.LastLoginAt,
                IsAdmin = user.IsAdmin,
                IsActive = user.IsActive
            };
        }
    }

    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public List<Category> UserPreferences { get; set; } = new List<Category>();
        public List<Category> AllCategories { get; set; } = new List<Category>();
    }

    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        
        // 分页属性
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int TotalItems { get; set; }
        
        // 搜索和筛选属性
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "registerDate";
        public bool? IsActive { get; set; }
        
        // 帮助器方法 - 生成排序链接，保留当前筛选条件
        public string GetSortUrl(string column)
        {
            bool isDescending = SortBy == column; // 当前是否按此列降序排列
            string newSortOrder = isDescending ? $"{column}Asc" : column;
            
            // 特殊情况：RegisterDate 的默认排序是降序
            if (column == "registerDate" && SortBy == "registerDate")
                newSortOrder = "registerDateAsc";
            else if (column == "registerDate" && SortBy == "registerDateAsc")
                newSortOrder = "registerDate";
            
            var searchTermParam = string.IsNullOrEmpty(SearchTerm) ? 
                string.Empty : $"&searchTerm={UrlEncoder.Default.Encode(SearchTerm)}";
            var isActiveParam = IsActive.HasValue ? 
                $"&isActive={IsActive.Value}" : string.Empty;
                
            return $"?page=1&sortBy={newSortOrder}{searchTermParam}{isActiveParam}";
        }
        
        // 帮助器方法 - 获取分页URL
        public string GetPageUrl(int pageNumber)
        {
            var searchTermParam = string.IsNullOrEmpty(SearchTerm) ? 
                string.Empty : $"&searchTerm={UrlEncoder.Default.Encode(SearchTerm)}";
            var isActiveParam = IsActive.HasValue ? 
                $"&isActive={IsActive.Value}" : string.Empty;
                
            return $"?page={pageNumber}&sortBy={SortBy}{searchTermParam}{isActiveParam}";
        }
        
        // 帮助器方法 - 获取活跃状态筛选URL
        public string GetActiveFilterUrl(bool? activeState)
        {
            var searchTermParam = string.IsNullOrEmpty(SearchTerm) ? 
                string.Empty : $"&searchTerm={UrlEncoder.Default.Encode(SearchTerm)}";
                
            if (activeState.HasValue)
                return $"?page=1&sortBy={SortBy}{searchTermParam}&isActive={activeState.Value}";
            else
                return $"?page=1&sortBy={SortBy}{searchTermParam}";
        }
    }
} 