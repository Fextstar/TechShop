using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryID { get; set; }

        [StringLength(255)]
        [Display(Name = "Hình ảnh")]
        public string ImageURL { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        [ForeignKey("ParentCategoryID")]
        public virtual Category ParentCategory { get; set; }

        public virtual ICollection<Category> SubCategories { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
