using System.ComponentModel.DataAnnotations;

namespace SocialSoftwareAPI.Models
{
    public sealed class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]

        public int Owner { get; set; } 

        [Required]

        public string Content { get; set; } = string.Empty;

        public string CreateDate { get; set; } = string.Empty;

        public string CreateTime { get; set; } = string.Empty;

    }
}
