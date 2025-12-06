using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDienThoai.Models
{
    public class SanPhamViewModel
    {
        public int MASP { get; set; }
        public int MALOAI { get; set; }
        public string TENSP { get; set; }
        public decimal GIABAN { get; set; }
        public string ANH { get; set; }   // chỉ tên file .webp
        public string HANGSP { get; set; }
        public int SOLUONG { get; set; }
    }
}