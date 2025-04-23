using System.ComponentModel.DataAnnotations;

namespace Campus_News_Feed.Models.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "请输入用户名")]
        [Display(Name = "用户名")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "请输入密码")]
        [Display(Name = "密码")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "记住我")]
        public bool RememberMe { get; set; }
    }
} 