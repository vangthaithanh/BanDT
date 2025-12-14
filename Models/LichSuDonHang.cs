using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDienThoai.Models
{
    public class LichSuDonHang
    {
        public int MaHD { get; set; }
        public DateTime NgayLap { get; set; }
        public decimal ThanhTien { get; set; }

        public string TrangThaiHoaDon { get; set; }     
        public string TrangThaiGiaoHang { get; set; }   
        public DateTime? NgayGiao { get; set; }

        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string Anh { get; set; }

        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
}