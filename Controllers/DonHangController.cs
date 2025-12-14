using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebDienThoai.Models;

namespace WebDienThoai.Controllers
{
    public class DonHangController : Controller
    {
        private string GetConnStr()
        {
            // lấy tên conn lưu trong Session khi login
            var connName = Session["ConnName"] as string;

            if (string.IsNullOrEmpty(connName))
            {
                // chưa login hoặc không set → mặc định khách
                connName = "Conn_Khach";
            }

            var cs = ConfigurationManager.ConnectionStrings[connName];
            if (cs == null)
            {
                // nếu cấu hình sai tên → fallback an toàn
                cs = ConfigurationManager.ConnectionStrings["Conn_Khach"];
            }

            return cs.ConnectionString;
        }
        // GET: DonHang
        public ActionResult LichSuMuaHang(string status = null)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Account");
            string tentk = Session["UserName"].ToString();
            string connStr = GetConnStr();
            var list = new List<LichSuDonHang>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("usp_DonHang_LichSu", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TENTK", tentk);
                cmd.Parameters.AddWithValue("@TrangThaiGiaoHang",
                    string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new LichSuDonHang
                        {
                            MaHD = Convert.ToInt32(rd["MAHD"]),
                            NgayLap = Convert.ToDateTime(rd["NGAYLAP"]),
                            ThanhTien = Convert.ToDecimal(rd["THANHTIEN"]),

                            TrangThaiHoaDon = rd["TRANGTHAI_HOADON"]?.ToString(),
                            TrangThaiGiaoHang = rd["TRANGTHAI_GIAOHANG"] == DBNull.Value ? null : rd["TRANGTHAI_GIAOHANG"].ToString(),
                            NgayGiao = rd["NGAYGIAO"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["NGAYGIAO"]),

                            MaSP = Convert.ToInt32(rd["MASP"]),
                            TenSP = rd["TENSP"]?.ToString(),
                            Anh = rd["ANH"]?.ToString(),

                            SoLuong = Convert.ToInt32(rd["SOLUONG"]),
                            DonGia = Convert.ToDecimal(rd["DONGIA"])
                        });
                    }
                }
            }
            ViewBag.CurrentStatus = status;
            return View(list);
        }
        public ActionResult ChiTiet(int id) 
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Account");
            string tentk = Session["UserName"].ToString();
            var list = new List<LichSuDonHang>();
            string connStr = GetConnStr();

            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("dbo.usp_DonHang_ChiTiet", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TENTK", tentk);
                cmd.Parameters.AddWithValue("@MAHD", id);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new LichSuDonHang
                        {
                            MaHD = Convert.ToInt32(rd["MAHD"]),
                            NgayLap = Convert.ToDateTime(rd["NGAYLAP"]),
                            ThanhTien = Convert.ToDecimal(rd["THANHTIEN"]),
                            TrangThaiHoaDon = rd["TRANGTHAI_HOADON"].ToString(),
                            TrangThaiGiaoHang = rd["TRANGTHAI_GIAOHANG"] == DBNull.Value ? null : rd["TRANGTHAI_GIAOHANG"].ToString(),
                            NgayGiao = rd["NGAYGIAO"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["NGAYGIAO"]),

                            MaSP = Convert.ToInt32(rd["MASP"]),
                            TenSP = rd["TENSP"].ToString(),
                            Anh = rd["ANH"].ToString(),
                            SoLuong = Convert.ToInt32(rd["SOLUONG"]),
                            DonGia = Convert.ToDecimal(rd["DONGIA"])
                        });
                    }
                }
            }
            if (list.Count == 0) return HttpNotFound(); 
            return View(list); 
        }



        public ActionResult Index()
        {
            return View();
        }
    }
}