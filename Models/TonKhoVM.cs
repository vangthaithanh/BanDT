using System;
using System.Collections.Generic;

namespace WebDienThoai.Models.ViewModels
{
    public class TonKhoVM
    {
        public string MAKHO { get; set; }
        public string TENKHO { get; set; }

        public List<WebDienThoai.Models.Kho> KhoList { get; set; } = new List<WebDienThoai.Models.Kho>();
        public List<WebDienThoai.Models.TonKho> Items { get; set; } = new List<WebDienThoai.Models.TonKho>();
    }

    public class PhieuNhapListVM
    {
        public string MAKHO { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }

        public List<WebDienThoai.Models.Kho> KhoList { get; set; } = new List<WebDienThoai.Models.Kho>();
        public List<WebDienThoai.Models.PhieuNhap> Items { get; set; } = new List<WebDienThoai.Models.PhieuNhap>();
    }

    public class PhieuNhapDetailVM
    {
        public WebDienThoai.Models.PhieuNhap PhieuNhap { get; set; }
        public List<WebDienThoai.Models.ChiTietPN> ChiTiet { get; set; } = new List<WebDienThoai.Models.ChiTietPN>();
    }

    public class CreatePhieuNhapItemVM
    {
        public string MASP { get; set; }
        public int SOLUONG { get; set; }
        public decimal GIANHAP { get; set; }
    }

    public class CreatePhieuNhapVM
    {
        public string MAKHO { get; set; }
        public string NHACUNGCAP { get; set; }
        public DateTime? NGAYNHAP { get; set; }

        public List<WebDienThoai.Models.Kho> KhoList { get; set; } = new List<WebDienThoai.Models.Kho>();
        public List<CreatePhieuNhapItemVM> Items { get; set; } = new List<CreatePhieuNhapItemVM>
        {
            new CreatePhieuNhapItemVM { SOLUONG = 1, GIANHAP = 1 }
        };
    }
}
