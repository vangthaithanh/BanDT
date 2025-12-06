using System;

namespace WebDienThoai.Models
{
    public class PhieuNhap
    {
        public int MAPHIEUNHAP { get; set; }
        public string ID { get; set; }           // người tạo (nếu có)
        public DateTime NGAYNHAP { get; set; }
        public string NHACUNGCAP { get; set; }
        public decimal TONGGIA { get; set; }
        public string MAKHO { get; set; }

        // view tiện hiển thị
        public string TENKHO { get; set; }
    }
}
