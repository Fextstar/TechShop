using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("Banners")]
    public class Banner
    {
        [Key]
        public int BannerID { get; set; }

        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Hình ảnh")]
        public string ImageURL { get; set; }

        [StringLength(255)]
        [Display(Name = "Link")]
        public string LinkURL { get; set; }

        [StringLength(50)]
        [Display(Name = "Vị trí")]
        public string Position { get; set; }

        [Display(Name = "Thứ tự")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Ngày kết thúc")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }
    }
}