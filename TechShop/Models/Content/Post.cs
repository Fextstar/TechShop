using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("Posts")]
    public class Post
    {
        [Key]
        public int PostID { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Slug")]
        public string Slug { get; set; }

        [Display(Name = "Nội dung")]
        [DataType(DataType.Html)]
        public string Content { get; set; }

        [StringLength(255)]
        [Display(Name = "Ảnh thumbnail")]
        public string ThumbnailURL { get; set; }

        [Required]
        [Display(Name = "Tác giả")]
        public int AuthorID { get; set; }

        [Display(Name = "Lượt xem")]
        public int ViewCount { get; set; }

        [Display(Name = "Đã xuất bản")]
        public bool IsPublished { get; set; }

        [Display(Name = "Ngày xuất bản")]
        public DateTime? PublishedDate { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey("AuthorID")]
        public virtual User Author { get; set; }
    }
}