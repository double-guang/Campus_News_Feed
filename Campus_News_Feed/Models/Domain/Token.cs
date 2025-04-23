using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Campus_News_Feed.Models.Domain
{
    public enum TokenType
    {
        Registration,
        Login
    }
    
    public class Token
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Value { get; set; } = string.Empty;

        [Required]
        public TokenType Type { get; set; }

        // 用户ID，注册令牌时可为null
        public int? UserId { get; set; }

        // 使用外键关联
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // 邮箱，主要用于注册令牌
        [Required]
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
    }
} 