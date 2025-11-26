using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("Contacts")]
    public class Contact
    {
        [Key]
        public int ContactID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(20)]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [StringLength(255)]
        [Display(Name = "Tiêu đề")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Tin nhắn")]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        [Display(Name = "Đã đọc")]
        public bool IsRead { get; set; }

        [Display(Name = "Đã trả lời")]
        public bool IsReplied { get; set; }

        [Display(Name = "Ngày gửi")]
        public DateTime CreatedDate { get; set; }
    }
}