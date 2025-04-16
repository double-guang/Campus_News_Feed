using Campus_News_Feed.Models.Domain;

namespace Campus_News_Feed.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        public List<Category> AllCategories { get; set; } = new List<Category>();
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
    }
} 