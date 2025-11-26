using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("ProductReviews")]
    public class ProductReview
    {
        [Key]
        public int ReviewID { get; set; }

        [Required]
        [Display(Name = "Sản phẩm")]
        public int ProductID { get; set; }

        [Required]
        [Display(Name = "Người dùng")]
        public int UserID { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá từ 1 đến 5 sao")]
        [Display(Name = "Số sao")]
        public int Rating { get; set; }

        [StringLength(200)]
        [Display(Name = "Tiêu đề đánh giá")]
        public string ReviewTitle { get; set; }

        [Display(Name = "Nội dung đánh giá")]
        [DataType(DataType.MultilineText)]
        public string ReviewContent { get; set; }

        [Display(Name = "Đã duyệt")]
        public bool IsApproved { get; set; }

        [Display(Name = "Ngày đánh giá")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}