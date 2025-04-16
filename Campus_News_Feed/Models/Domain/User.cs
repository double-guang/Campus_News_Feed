using System.ComponentModel.DataAnnotations;

namespace Campus_News_Feed.Models.Domain
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime RegisteredAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public bool IsAdmin { get; set; } = false;

        // 导航属性
        public virtual ICollection<UserPreference>? Preferences { get; set; }
    }
} 