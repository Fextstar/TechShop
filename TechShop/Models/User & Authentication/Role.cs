using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Tên vai trò")]
        public string RoleName { get; set; }

        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; }
    }
}
