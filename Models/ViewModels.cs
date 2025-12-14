using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDienThoai.Models.ViewModels
{
    public class CheckoutPreviewVM
    {
        public List<WebDienThoai.Models.GioHang> Items { get; set; } = new List<WebDienThoai.Models.GioHang>();
        public int TongSanPham => Items?.Sum(x => x.SOLUONG) ?? 0;
        public decimal TongTien => Items?.Sum(x => x.ThanhTien) ?? 0m;
    }
}