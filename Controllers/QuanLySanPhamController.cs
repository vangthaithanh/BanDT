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
    public class QuanLySanPhamController : Controller
    {
        // GET: QuanLySanPham
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
        public ActionResult Index(int? maloai, string keyword)
        {
            // breadcrumb
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Quản lý sản phẩm" }
            };

            // Load danh sách loại sản phẩm
            ViewBag.Loai = GetAllLoai();

            List<SanPhamViewModel> list = new List<SanPhamViewModel>();

            using (SqlConnection conn = new SqlConnection(GetConnStr()))
            using (SqlCommand cmd = new SqlCommand("SP_SanPham_GetAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new SanPhamViewModel
                        {
                            MASP = Convert.ToInt32(rd["MASP"]),
                            TENSP = rd["TENSP"].ToString(),
                            GIABAN = Convert.ToDecimal(rd["GIABAN"]),
                            ANH = rd["ANH"].ToString(),
                            MALOAI = Convert.ToInt32(rd["MALOAI"]),
                            SOLUONG = Convert.ToInt32(rd["SOLUONG"])  // lấy từ TONKHO
                        });
                    }
                }
            }

            // ===== LỌC THEO MÃ LOẠI =====
            if (maloai.HasValue)
            {
                list = list.Where(x => x.MALOAI == maloai.Value).ToList();
            }

            // ===== LỌC THEO TỪ KHÓA =====
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(x => x.TENSP.Contains(keyword.Trim())).ToList();
            }

            return View(list);
        }

        private List<LoaiSP> GetAllLoai()
        {
            var list = new List<LoaiSP>();

            using (SqlConnection conn = new SqlConnection(GetConnStr()))
            using (SqlCommand cmd = new SqlCommand("SELECT MALOAI, TENLOAI FROM LOAISP", conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new LoaiSP
                        {
                            MALOAI = rd["MALOAI"].GetHashCode(),
                            TENLOAI = rd["TENLOAI"].ToString()
                        });
                    }
                }
            }
            return list;
        }
        private List<SanPhamViewModel> LocSanPhamTheoLoai(int? maloai)
        {
            var list = new List<SanPhamViewModel>();

            using (SqlConnection conn = new SqlConnection(GetConnStr()))
            using (SqlCommand cmd = new SqlCommand("SP_DuyetSanPham_TheoLoai", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaLoai", maloai ?? (object)DBNull.Value);

                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new SanPhamViewModel
                        {
                            MASP = rd["MASP"].GetHashCode(),
                            TENSP = rd["TENSP"].ToString(),
                            GIABAN = Convert.ToDecimal(rd["GIABAN"]),
                            ANH = rd["ANH"].ToString(),
                            MALOAI = rd["MALOAI"].GetHashCode()
                        });
                    }
                }
            }

            return list;
        }

    }
}