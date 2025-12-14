using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebDienThoai.Models
{
    public class DoiMatKhau
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải từ 6 ký tự.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
        [Compare("NewPassword", ErrorMessage = "Nhập lại mật khẩu không khớp.")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }
    }
}