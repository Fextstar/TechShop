using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("SiteSettings")]
    public class SiteSetting
    {
        [Key]
        public int SettingID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Khóa")]
        public string SettingKey { get; set; }

        [Display(Name = "Giá trị")]
        public string SettingValue { get; set; }

        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime UpdatedDate { get; set; }
    }
}