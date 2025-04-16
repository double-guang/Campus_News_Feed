using System.ComponentModel.DataAnnotations;

namespace Campus_News_Feed.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "请输入您的学校邮箱")]
        [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
        [Display(Name = "邮箱")]
        public string Email { get; set; } = string.Empty;
    }
} 