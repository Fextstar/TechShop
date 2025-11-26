using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        [Key]
        public int ImageID { get; set; }

        [Required]
        [Display(Name = "Sản phẩm")]
        public int ProductID { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "URL hình ảnh")]
        public string ImageURL { get; set; }

        [Display(Name = "Ảnh chính")]
        public bool IsPrimary { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
    }
}