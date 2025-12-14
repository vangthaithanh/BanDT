using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDienThoai.Models
{
    public class HoaDonViewModel
    {
        public int MAHD { get; set; }
        public DateTime NGAYLAPHD { get; set; }
        public decimal TONGTIEN { get; set; }
        public string TINHTRANG { get; set; }
        public string PHUONGTHUCTHANHTOAN { get; set; }
        public string TENKH { get; set; }
        public string SDT { get; set; }
        public string DIACHI { get; set; }
    }

    // Model cho chi tiết sản phẩm trong hóa đơn
    public class ChiTietHoaDonItem
    {
        public int MASP { get; set; }
        public string TENSP { get; set; }
        public string ANH { get; set; }
        public int SOLUONG { get; set; }
        public decimal DONGIA { get; set; }
        public decimal THANHTIEN { get; set; }
    }

    // Model tổng hợp cho trang chi tiết hóa đơn
    public class HoaDonChiTietViewModel
    {
        public HoaDonViewModel HoaDon { get; set; }
        public List<ChiTietHoaDonItem> ChiTiet { get; set; }
    }
}