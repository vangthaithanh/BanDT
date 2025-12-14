using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDienThoai.Models
{
    public class UuDai
    {
        public int MaPKM { get; set; }
        public string LoaiPhieu { get; set; }
        public int GiaTri { get; set; }
        public string DieuKien { get; set; }
        public DateTime NgayHetHan { get; set; }
    }

}
