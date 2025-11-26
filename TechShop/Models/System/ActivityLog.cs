using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("ActivityLogs")]
    public class ActivityLog
    {
        [Key]
        public int LogID { get; set; }

        [Display(Name = "Người dùng")]
        public int? UserID { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Hành động")]
        public string Action { get; set; }

        [StringLength(100)]
        [Display(Name = "Tên bảng")]
        public string TableName { get; set; }

        [Display(Name = "ID bản ghi")]
        public int? RecordID { get; set; }

        [Display(Name = "Giá trị cũ")]
        public string OldValue { get; set; }

        [Display(Name = "Giá trị mới")]
        public string NewValue { get; set; }

        [StringLength(50)]
        [Display(Name = "Địa chỉ IP")]
        public string IPAddress { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}