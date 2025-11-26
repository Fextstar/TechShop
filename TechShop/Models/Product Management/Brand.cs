using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("Brands")]
    public class Brand
    {
        [Key]
        public int BrandID { get; set; }

        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên thương hiệu")]
        public string BrandName { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [StringLength(255)]
        [Display(Name = "Logo")]
        public string LogoURL { get; set; }

        [StringLength(255)]
        [Display(Name = "Website")]
        public string Website { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; }
    }
}