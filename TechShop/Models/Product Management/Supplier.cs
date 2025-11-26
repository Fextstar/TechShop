using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("Suppliers")]
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên nhà cung cấp")]
        public string SupplierName { get; set; }

        [StringLength(100)]
        [Display(Name = "Người liên hệ")]
        public string ContactPerson { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(20)]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; }
    }
}