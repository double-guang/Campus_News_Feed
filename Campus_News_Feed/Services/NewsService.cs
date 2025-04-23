using Campus_News_Feed.Data;
using Campus_News_Feed.Models.Domain;
using Campus_News_Feed.Models.ViewModels;
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

        public async Task<PaginatedList<News>> GetPaginatedNewsAsync(int pageIndex, int pageSize)
        {
            try
            {
                var query = _context.News
                    .Include(n => n.Category)
                    .OrderByDescending(n => n.CreatedAt)
                    .AsQueryable();

                return await PaginatedList<News>.CreateAsync(query, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分页新闻列表时出错");
                return new PaginatedList<News>(new List<News>(), 0, pageIndex, pageSize);
            }
        }

        public async Task<PaginatedList<News>> GetPaginatedNewsByCategoryAsync(int categoryId, int pageIndex, int pageSize)
        {
            try
            {
                var query = _context.News
                    .Include(n => n.Category)
                    .Where(n => n.CategoryId == categoryId)
                    .OrderByDescending(n => n.ClickCount)
                    .ThenByDescending(n => n.CreatedAt)
                    .AsQueryable();

                return await PaginatedList<News>.CreateAsync(query, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类{CategoryId}的分页新闻列表时出错", categoryId);
                return new PaginatedList<News>(new List<News>(), 0, pageIndex, pageSize);
            }
        }

        public async Task<PaginatedList<News>> GetPaginatedRecommendedNewsAsync(int userId, int pageIndex, int pageSize)
        {
            try
            {
                // 获取当前时间
                var now = DateTime.UtcNow;
                
                // 获取用户偏好的新闻类别
                var userPreferredCategories = await _context.UserPreferences
                    .Where(up => up.UserId == userId)
                    .Select(up => up.CategoryId)
                    .ToListAsync();

                // 获取所有新闻
                var allNews = await _context.News
                    .Include(n => n.Category)
                    .ToListAsync();
                
                List<News> sortedNews;
                
                if (!userPreferredCategories.Any())
                {
                    // 如果用户没有设置偏好，使用通用的新闻时效性排序算法
                    sortedNews = allNews
                        .Select(n => new
                        {
                            News = n,
                            // 时间新鲜度得分 - 使用更陡峭的衰减函数
                            TimeScore = Math.Exp(-0.25 * (now - n.CreatedAt).TotalDays),
                            // 近期新闻额外加分（阶梯式）
                            RecentBonus = (now - n.CreatedAt).TotalHours <= 6 ? 5.0 :  // 6小时内
                                         (now - n.CreatedAt).TotalHours <= 12 ? 3.0 :  // 12小时内
                                         (now - n.CreatedAt).TotalHours <= 24 ? 2.0 :  // 24小时内
                                         (now - n.CreatedAt).TotalDays <= 2 ? 1.0 : 0, // 2天内
                            // 点击量得分 - 使用对数缩放
                            ClickScore = Math.Log(n.ClickCount + 1)
                        })
                        // 综合得分公式 - 时间占绝对主导地位
                        .OrderByDescending(x => x.RecentBonus + x.TimeScore * 0.85 + x.ClickScore * 0.15)
                        .Select(x => x.News)
                        .ToList();
                }
                else
                {
                    // 根据用户偏好和时间新鲜度进行个性化推荐
                    sortedNews = allNews
                        .Select(n => new
                        {
                            News = n,
                            // 偏好匹配得分
                            PreferenceScore = userPreferredCategories.Contains(n.CategoryId) ? 3.0 : 0.0,
                            // 时间新鲜度得分 - 使用更陡峭的衰减函数
                            TimeScore = Math.Exp(-0.25 * (now - n.CreatedAt).TotalDays),
                            // 近期新闻额外加分（阶梯式）
                            RecentBonus = (now - n.CreatedAt).TotalHours <= 6 ? 5.0 :  // 6小时内
                                         (now - n.CreatedAt).TotalHours <= 12 ? 3.0 :  // 12小时内
                                         (now - n.CreatedAt).TotalHours <= 24 ? 2.0 :  // 24小时内
                                         (now - n.CreatedAt).TotalDays <= 2 ? 1.0 : 0, // 2天内
                            // 点击量得分 - 使用对数缩放
                            ClickScore = Math.Log(n.ClickCount + 1),
                            // 是否是用户偏好类别
                            IsPreferred = userPreferredCategories.Contains(n.CategoryId)
                        })
                        // 多层排序：先按偏好分组，再按综合得分排序
                        .OrderByDescending(x => x.IsPreferred)
                        .ThenByDescending(x => x.RecentBonus + x.TimeScore * 0.85 + x.ClickScore * 0.15 + x.PreferenceScore * 0.5)
                        .Select(x => x.News)
                        .ToList();
                }

                // 创建分页结果
                int totalCount = sortedNews.Count;
                var paginatedItems = sortedNews
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                return new PaginatedList<News>(paginatedItems, totalCount, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户{UserId}的推荐分页新闻列表时出错", userId);
                return new PaginatedList<News>(new List<News>(), 0, pageIndex, pageSize);
            }
        }

        public async Task<IEnumerable<News>> GetAllNewsAsync()
        {
            return await _context.News
                .Include(n => n.Category)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<News>> GetNewsByCategoryAsync(int categoryId)
        {
            return await _context.News
                .Include(n => n.Category)
                .Where(n => n.CategoryId == categoryId)
                .OrderByDescending(n => n.ClickCount)
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<News>> GetRecommendedNewsAsync(int userId)
        {
            try
            {
                // 获取当前时间
                var now = DateTime.UtcNow;
                
            // 获取用户偏好的新闻类别
            var userPreferredCategories = await _context.UserPreferences
                .Where(up => up.UserId == userId)
                .Select(up => up.CategoryId)
                .ToListAsync();

                // 获取新闻列表
                var query = _context.News.Include(n => n.Category);
                var allNews = await query.ToListAsync();
                
            if (!userPreferredCategories.Any())
            {
                    // 如果用户没有设置偏好，使用通用的新闻时效性排序算法
                    var timeBasedRecommendation = allNews
                        .Select(n => new
                        {
                            News = n,
                            // 时间新鲜度得分 - 使用更陡峭的衰减函数
                            TimeScore = Math.Exp(-0.25 * (now - n.CreatedAt).TotalDays),
                            // 近期新闻额外加分（阶梯式）
                            RecentBonus = (now - n.CreatedAt).TotalHours <= 6 ? 5.0 :  // 6小时内
                                         (now - n.CreatedAt).TotalHours <= 12 ? 3.0 :  // 12小时内
                                         (now - n.CreatedAt).TotalHours <= 24 ? 2.0 :  // 24小时内
                                         (now - n.CreatedAt).TotalDays <= 2 ? 1.0 : 0, // 2天内
                            // 点击量得分 - 使用对数缩放
                            ClickScore = Math.Log(n.ClickCount + 1)
                        })
                        // 综合得分公式 - 时间占绝对主导地位
                        .OrderByDescending(x => x.RecentBonus + x.TimeScore * 0.85 + x.ClickScore * 0.15)
                        .Select(x => x.News)
                    .Take(10)
                        .ToList();
                    
                    return timeBasedRecommendation;
            }

                // 根据用户偏好和时间新鲜度进行个性化推荐
                var personalizedRecommendation = allNews
                    .Select(n => new
                    {
                        News = n,
                        // 偏好匹配得分
                        PreferenceScore = userPreferredCategories.Contains(n.CategoryId) ? 3.0 : 0.0,
                        // 时间新鲜度得分 - 使用更陡峭的衰减函数
                        TimeScore = Math.Exp(-0.25 * (now - n.CreatedAt).TotalDays),
                        // 近期新闻额外加分（阶梯式）
                        RecentBonus = (now - n.CreatedAt).TotalHours <= 6 ? 5.0 :  // 6小时内
                                     (now - n.CreatedAt).TotalHours <= 12 ? 3.0 :  // 12小时内
                                     (now - n.CreatedAt).TotalHours <= 24 ? 2.0 :  // 24小时内
                                     (now - n.CreatedAt).TotalDays <= 2 ? 1.0 : 0, // 2天内
                        // 点击量得分 - 使用对数缩放
                        ClickScore = Math.Log(n.ClickCount + 1),
                        // 是否是用户偏好类别
                        IsPreferred = userPreferredCategories.Contains(n.CategoryId)
                    })
                    // 多层排序：先按偏好分组，再按综合得分排序
                    .OrderByDescending(x => x.IsPreferred)
                    .ThenByDescending(x => x.RecentBonus + x.TimeScore * 0.85 + x.ClickScore * 0.15 + x.PreferenceScore * 0.5)
                    .Select(x => x.News)
                .Take(20)
                    .ToList();
                
                return personalizedRecommendation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户推荐新闻时出错: {UserId}", userId);
                return new List<News>();
            }
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

        public async Task<bool> UpdateNewsAsync(News news)
        {
            try
            {
                var existingNews = await _context.News.FindAsync(news.Id);
                if (existingNews == null)
                {
                    return false;
                }

                existingNews.Title = news.Title;
                existingNews.Content = news.Content;
                existingNews.CategoryId = news.CategoryId;
                existingNews.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新新闻时出错: {NewsId}", news.Id);
                return false;
            }
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

        public async Task<bool> IncrementClickCountAsync(int newsId)
        {
            var news = await _context.News.FindAsync(newsId);
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
        
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public AppDbContext GetDbContext()
        {
            return _context;
        }

        public async Task<IEnumerable<News>> GetHotNewsAsync(int count)
        {
            try
            {
                // 获取当前时间
                var now = DateTime.UtcNow;
                
                // 获取所有新闻
                var allNews = await _context.News
                    .Include(n => n.Category)
                    .ToListAsync();
                
                // 使用更注重时效性的算法进行排序 - 新闻推荐
                // 时间权重：85%，点击量权重：15%
                var todayRecommended = allNews
                    .Select(n => new
                    {
                        News = n,
                        // 时间新鲜度得分 - 使用更陡峭的衰减函数
                        // 让超过3天的新闻显著降低权重
                        TimeScore = Math.Exp(-0.25 * (now - n.CreatedAt).TotalDays),
                        // 近期新闻额外加分（阶梯式）
                        RecentBonus = (now - n.CreatedAt).TotalHours <= 6 ? 5.0 :  // 6小时内
                                     (now - n.CreatedAt).TotalHours <= 12 ? 3.0 :  // 12小时内
                                     (now - n.CreatedAt).TotalHours <= 24 ? 2.0 :  // 24小时内
                                     (now - n.CreatedAt).TotalDays <= 2 ? 1.0 : 0, // 2天内
                        // 点击量得分 - 使用对数缩放
                        ClickScore = Math.Log(n.ClickCount + 1)
                    })
                    // 综合得分公式 - 时间占绝对主导地位
                    .OrderByDescending(x => x.RecentBonus + x.TimeScore * 0.85 + x.ClickScore * 0.15)
                    .Select(x => x.News)
                    .Take(count)
                    .ToList();
                
                return todayRecommended;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取今日推荐新闻时出错");
                return new List<News>();
            }
        }

        public async Task<int> GetNewsByCategoryCountAsync(int categoryId)
        {
            try
            {
                return await _context.News
                    .Where(n => n.CategoryId == categoryId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类新闻计数时出错: {CategoryId}", categoryId);
                return 0;
            }
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建分类时出错: {CategoryName}", category.Name);
                throw;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                var existingCategory = await _context.Categories.FindAsync(category.Id);
                if (existingCategory == null)
                {
                    return false;
                }

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新分类时出错: {CategoryId}", category.Id);
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return false;
                }

                // 检查分类下是否有新闻
                bool hasNews = await _context.News.AnyAsync(n => n.CategoryId == id);
                if (hasNews)
                {
                    _logger.LogWarning("尝试删除包含新闻的分类: {CategoryId}", id);
                    return false;
                }

                // 检查分类是否被用户偏好引用
                bool hasUserPreferences = await _context.UserPreferences.AnyAsync(up => up.CategoryId == id);
                if (hasUserPreferences)
                {
                    // 删除相关的用户偏好
                    var userPreferences = await _context.UserPreferences
                        .Where(up => up.CategoryId == id)
                        .ToListAsync();
                    
                    _context.UserPreferences.RemoveRange(userPreferences);
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除分类时出错: {CategoryId}", id);
                return false;
            }
        }

        public async Task<int> GetNewsCountAsync()
        {
            return await _context.News.CountAsync();
        }

        public async Task<PaginatedList<News>> GetFilteredNewsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            int? categoryId = null, 
            DateTime? dateFrom = null, 
            DateTime? dateTo = null,
            NewsSortOption sortOption = NewsSortOption.Comprehensive)
        {
            try
            {
                // 基本查询
                IQueryable<News> query = _context.News.Include(n => n.Category);
                
                // 应用搜索条件
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(n => 
                        n.Title.ToLower().Contains(searchTerm) || 
                        n.Content.ToLower().Contains(searchTerm));
                }
                
                // 应用分类筛选
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(n => n.CategoryId == categoryId.Value);
                }
                
                // 应用日期范围筛选
                if (dateFrom.HasValue)
                {
                    // 确保从当天的开始时间开始筛选
                    var startDate = dateFrom.Value.Date;
                    query = query.Where(n => n.CreatedAt >= startDate);
                }
                
                if (dateTo.HasValue)
                {
                    // 确保包含当天的结束时间
                    var endDate = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(n => n.CreatedAt <= endDate);
                }
                
                // 应用排序
                switch (sortOption)
                {
                    case NewsSortOption.TimeDesc:
                        query = query.OrderByDescending(n => n.CreatedAt);
                        break;
                    case NewsSortOption.TimeAsc:
                        query = query.OrderBy(n => n.CreatedAt);
                        break;
                    case NewsSortOption.Comprehensive:
                    default:
                        // 综合排序：先按点击量，再按创建时间
                        query = query.OrderByDescending(n => n.ClickCount)
                                    .ThenByDescending(n => n.CreatedAt);
                        break;
                }
                
                return await PaginatedList<News>.CreateAsync(query, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取筛选的新闻列表时出错");
                return new PaginatedList<News>(new List<News>(), 0, pageIndex, pageSize);
            }
        }

        // 实现支持排序的新方法
        public async Task<PaginatedList<News>> GetPaginatedNewsSortedAsync(int pageIndex, int pageSize, NewsSortOption sortOption, int? userId = null)
        {
            try
            {
                IQueryable<News> query = _context.News.Include(n => n.Category);
                
                // 根据排序选项应用不同的排序规则
                switch (sortOption)
                {
                    case NewsSortOption.TimeDesc:
                        // 按时间降序排序（最新的在前）
                        query = query.OrderByDescending(n => n.CreatedAt);
                        break;
                    
                    case NewsSortOption.TimeAsc:
                        // 按时间升序排序（最早的在前）
                        query = query.OrderBy(n => n.CreatedAt);
                        break;
                    
                    case NewsSortOption.Comprehensive:
                    default:
                        // 获取当前时间
                        var now = DateTime.UtcNow;
                        
                        // 根据用户状态应用不同的综合排序逻辑
                        if (userId.HasValue)
                        {
                            // 已登录用户：检查是否有偏好设置
                            var userPreferences = await _context.UserPreferences
                                .Where(up => up.UserId == userId.Value)
                                .Select(up => up.CategoryId)
                                .ToListAsync();
                            
                            if (userPreferences.Any())
                            {
                                // 使用本地计算进行排序
                                var news = await query.ToListAsync();
                                var sortedNews = news
                                    .Select(n => new
                                    {
                                        News = n,
                                        // 用户偏好匹配加成
                                        PreferenceScore = userPreferences.Contains(n.CategoryId) ? 3.0 : 0.0,
                                        // 时间新鲜度得分 - 使用更陡峭的衰减函数
                                        TimeScore = Math.Exp(-0.25 * (now - n.CreatedAt).TotalDays),
                                        // 近期新闻额外加分（阶梯式）
                                        RecentBonus = (now - n.CreatedAt).TotalHours <= 6 ? 5.0 :  // 6小时内
                                                     (now - n.CreatedAt).TotalHours <= 12 ? 3.0 :  // 12小时内
                                                     (now - n.CreatedAt).TotalHours <= 24 ? 2.0 :  // 24小时内
                                                     (now - n.CreatedAt).TotalDays <= 2 ? 1.0 : 0, // 2天内
                                        // 点击量得分 - 使用对数缩放
                                        ClickScore = Math.Log(n.ClickCount + 1),
                                        // 是否属于偏好类别 - 用于分组排序
                                        IsPreferred = userPreferences.Contains(n.CategoryId)
                                    })
                                    // 多层排序：先按偏好分组，再按综合得分排序
                                    .OrderByDescending(x => x.IsPreferred)
                                    .ThenByDescending(x => x.RecentBonus + x.TimeScore * 0.85 + x.ClickScore * 0.15 + x.PreferenceScore * 0.5)
                                    .Select(x => x.News)
                                    .ToList();
                                
                                // 创建分页结果
                                int count = sortedNews.Count;
                                var pagedItems = sortedNews
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();
                                
                                return new PaginatedList<News>(pagedItems, count, pageIndex, pageSize);
                            }
                            else
                            {
                                // 用户没有设置偏好，使用通用的新闻时效性排序算法
                                var news = await query.ToListAsync();
                                var sortedNews = news
                                    .Select(n => new
                                    {
                                        News = n,
                                        // 时间新鲜度得分 - 使用更陡峭的衰减函数
                                        TimeScore = Math.Exp(-0.25 * (now - n.CreatedAt).TotalDays),
                                        // 近期新闻额外加分（阶梯式）
                                        RecentBonus = (now - n.CreatedAt).TotalHours <= 6 ? 5.0 :  // 6小时内
                                                     (now - n.CreatedAt).TotalHours <= 12 ? 3.0 :  // 12小时内
                                                     (now - n.CreatedAt).TotalHours <= 24 ? 2.0 :  // 24小时内
                                                     (now - n.CreatedAt).TotalDays <= 2 ? 1.0 : 0, // 2天内
                                        // 点击量得分 - 使用对数缩放
                                        ClickScore = Math.Log(n.ClickCount + 1)
                                    })
                                    // 综合得分公式 - 时间占绝对主导地位
                                    .OrderByDescending(x => x.RecentBonus + x.TimeScore * 0.85 + x.ClickScore * 0.15)
                                    .Select(x => x.News)
                                    .ToList();
                                
                                // 创建分页结果
                                int count = sortedNews.Count;
                                var pagedItems = sortedNews
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();
                                
                                return new PaginatedList<News>(pagedItems, count, pageIndex, pageSize);
                            }
                        }
                        else
                        {
                            // 游客：使用通用的新闻时效性排序算法
                            var news = await query.ToListAsync();
                            var sortedNews = news
                                .Select(n => new
                                {
                                    News = n,
                                    // 时间新鲜度得分 - 使用更陡峭的衰减函数
                                    TimeScore = Math.Exp(-0.25 * (now - n.CreatedAt).TotalDays),
                                    // 近期新闻额外加分（阶梯式）
                                    RecentBonus = (now - n.CreatedAt).TotalHours <= 6 ? 5.0 :  // 6小时内
                                                 (now - n.CreatedAt).TotalHours <= 12 ? 3.0 :  // 12小时内
                                                 (now - n.CreatedAt).TotalHours <= 24 ? 2.0 :  // 24小时内
                                                 (now - n.CreatedAt).TotalDays <= 2 ? 1.0 : 0, // 2天内
                                    // 点击量得分 - 使用对数缩放
                                    ClickScore = Math.Log(n.ClickCount + 1)
                                })
                                // 综合得分公式 - 时间占绝对主导地位
                                .OrderByDescending(x => x.RecentBonus + x.TimeScore * 0.85 + x.ClickScore * 0.15)
                                .Select(x => x.News)
                                .ToList();
                            
                            // 创建分页结果
                            int count = sortedNews.Count;
                            var pagedItems = sortedNews
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();
                            
                            return new PaginatedList<News>(pagedItems, count, pageIndex, pageSize);
                        }
                }

                return await PaginatedList<News>.CreateAsync(query, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取排序分页新闻列表时出错");
                return new PaginatedList<News>(new List<News>(), 0, pageIndex, pageSize);
            }
        }

        public async Task<PaginatedList<News>> GetPaginatedNewsByCategorySortedAsync(int categoryId, int pageIndex, int pageSize, NewsSortOption sortOption, int? userId = null)
        {
            try
            {
                IQueryable<News> query = _context.News
                    .Include(n => n.Category)
                    .Where(n => n.CategoryId == categoryId);
                
                // 根据排序选项应用不同的排序规则
                switch (sortOption)
                {
                    case NewsSortOption.TimeDesc:
                        // 按时间降序排序（最新的在前）
                        query = query.OrderByDescending(n => n.CreatedAt);
                        break;
                    
                    case NewsSortOption.TimeAsc:
                        // 按时间升序排序（最早的在前）
                        query = query.OrderBy(n => n.CreatedAt);
                        break;
                    
                    case NewsSortOption.Comprehensive:
                    default:
                        // 获取当前时间
                        var now = DateTime.UtcNow;
                        
                        // 根据用户状态应用不同的综合排序逻辑
                        if (userId.HasValue)
                        {
                            // 已登录用户：检查是否有偏好设置
                            var userPreferences = await _context.UserPreferences
                                .Where(up => up.UserId == userId.Value)
                                .Select(up => up.CategoryId)
                                .ToListAsync();
                            
                            // 检查当前分类是否是用户的偏好之一
                            bool isCategoryPreferred = userPreferences.Contains(categoryId);
                            
                            var news = await query.ToListAsync();
                            var sortedNews = news
                                .Select(n => new
                                {
                                    News = n,
                                    // 如果此分类是用户偏好，提供额外加成
                                    PreferenceScore = isCategoryPreferred ? 4.0 : 0.0,
                                    // 点击量分数 - 使用对数缩放
                                    ClickScore = Math.Log(n.ClickCount + 1),
                                    // 时间新鲜度分数 - 指数衰减
                                    TimeScore = Math.Exp(-0.05 * (now - n.CreatedAt).TotalDays)
                                })
                                .OrderByDescending(x => x.PreferenceScore + x.ClickScore * 0.35 + x.TimeScore * 0.65)
                                .Select(x => x.News)
                                .ToList();
                            
                            // 创建分页结果
                            int count = sortedNews.Count;
                            var pagedItems = sortedNews
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();
                            
                            return new PaginatedList<News>(pagedItems, count, pageIndex, pageSize);
                        }
                        else
                        {
                            // 游客：使用通用的新闻时效性排序算法
                            var news = await query.ToListAsync();
                            var sortedNews = news
                                .Select(n => new
                                {
                                    News = n,
                                    // 时间新鲜度得分 - 使用更陡峭的衰减函数
                                    TimeScore = Math.Exp(-0.25 * (now - n.CreatedAt).TotalDays),
                                    // 近期新闻额外加分（阶梯式）
                                    RecentBonus = (now - n.CreatedAt).TotalHours <= 6 ? 5.0 :  // 6小时内
                                                 (now - n.CreatedAt).TotalHours <= 12 ? 3.0 :  // 12小时内
                                                 (now - n.CreatedAt).TotalHours <= 24 ? 2.0 :  // 24小时内
                                                 (now - n.CreatedAt).TotalDays <= 2 ? 1.0 : 0, // 2天内
                                    // 点击量得分 - 使用对数缩放
                                    ClickScore = Math.Log(n.ClickCount + 1)
                                })
                                // 综合得分公式 - 时间占绝对主导地位
                                .OrderByDescending(x => x.RecentBonus + x.TimeScore * 0.85 + x.ClickScore * 0.15)
                                .Select(x => x.News)
                                .ToList();
                            
                            // 创建分页结果
                            int count = sortedNews.Count;
                            var pagedItems = sortedNews
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();
                            
                            return new PaginatedList<News>(pagedItems, count, pageIndex, pageSize);
                        }
                }

                return await PaginatedList<News>.CreateAsync(query, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类{CategoryId}的排序分页新闻列表时出错", categoryId);
                return new PaginatedList<News>(new List<News>(), 0, pageIndex, pageSize);
            }
        }
    }
} 