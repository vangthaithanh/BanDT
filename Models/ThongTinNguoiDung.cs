using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDienThoai.Models
{
    public class ThongTinNguoiDung
    {
        public string TenTK { get; set; }
        public int Id { get; set; }

        public string HoTen { get; set; }
        public string SDT { get; set; } 
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string GioiTinh { get; set; }
        public string DanToc { get; set; }

        public bool IsNhanVien { get; set; }
        public string ChucVu { get; set; }
        public System.DateTime? NgayVaoLam { get; set; }
        public int? NvTrangThai { get; set; }
        public int TongDonDaMua { get; set; }

        public int? Diem { get; set; }

        // hiển thị thêm từ Session (không bắt buộc)
        public string RoleCode { get; set; }
    }
}