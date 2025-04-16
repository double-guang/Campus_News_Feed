using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Campus_News_Feed.Models.Domain
{
    public class Token
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Value { get; set; } = string.Empty;

        [Required]
        public TokenType Type { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
    }

    public enum TokenType
    {
        Registration,
        Login
    }
} 