using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDienThoai.Models
{
    public class XemChiTietModel
    {
        public int MASP { get; set; }
        public string TENSP { get; set; }
        public decimal GIABAN { get; set; }
        public string ANH { get; set; }   // chỉ tên file .webp
        public double? KICHTHUOC { get; set; }
        public int MALOAI { get; set; }
        public string CONGNGHE { get; set; }
        public int? BONHO { get; set; }
        public string CAMERA { get; set; }
    }
}