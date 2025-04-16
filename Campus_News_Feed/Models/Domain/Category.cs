using System.ComponentModel.DataAnnotations;

namespace Campus_News_Feed.Models.Domain
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        // 导航属性
        public virtual ICollection<News>? News { get; set; }
        public virtual ICollection<UserPreference>? UserPreferences { get; set; }
    }
} 