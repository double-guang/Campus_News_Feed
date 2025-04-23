using System.ComponentModel.DataAnnotations;
using Campus_News_Feed.Models.Domain;

namespace Campus_News_Feed.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "分类名称不能为空")]
        [MaxLength(50, ErrorMessage = "分类名称最多50个字符")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200, ErrorMessage = "分类描述最多200个字符")]
        public string? Description { get; set; }
        
        public int NewsCount { get; set; }
        
        public static CategoryViewModel FromCategory(Category category, int newsCount = 0)
        {
            return new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                NewsCount = newsCount
            };
        }
    }
    
    public class CategoryCreateViewModel
    {
        [Required(ErrorMessage = "分类名称不能为空")]
        [MaxLength(50, ErrorMessage = "分类名称最多50个字符")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200, ErrorMessage = "分类描述最多200个字符")]
        public string? Description { get; set; }
    }
    
    public class CategoryEditViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "分类名称不能为空")]
        [MaxLength(50, ErrorMessage = "分类名称最多50个字符")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200, ErrorMessage = "分类描述最多200个字符")]
        public string? Description { get; set; }
    }
    
    public class CategoryListViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }
} 