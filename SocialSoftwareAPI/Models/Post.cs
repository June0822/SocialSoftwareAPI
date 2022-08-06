using System.ComponentModel.DataAnnotations;

namespace SocialSoftwareAPI.Models
{
    public sealed class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]

        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(length:100000)]

        public string Content { get; set; } = string.Empty;

    }
}
