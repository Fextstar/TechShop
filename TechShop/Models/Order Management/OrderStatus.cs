using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("OrderStatus")]
    public class OrderStatus
    {
        [Key]
        public int StatusID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Tên trạng thái")]
        public string StatusName { get; set; }

        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; }
    }
}