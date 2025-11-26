using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("FAQs")]
    public class FAQ
    {
        [Key]
        public int FAQID { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Câu hỏi")]
        public string Question { get; set; }

        [Required]
        [Display(Name = "Câu trả lời")]
        [DataType(DataType.MultilineText)]
        public string Answer { get; set; }

        [StringLength(100)]
        [Display(Name = "Danh mục")]
        public string CategoryFAQ { get; set; }

        [Display(Name = "Thứ tự")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }
    }
}