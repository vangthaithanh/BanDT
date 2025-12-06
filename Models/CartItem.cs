using System;

namespace WebDienThoai.Models
{
    
    public class GioHang
    {
        public int MASP { get; set; }
        public string TENSP { get; set; }
        public string ANH { get; set; }
        public int SOLUONG { get; set; }
        public decimal DONGIA { get; set; }
        public decimal ThanhTien => DONGIA * SOLUONG;
    }
}