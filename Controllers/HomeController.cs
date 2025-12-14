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
    public class HomeController : Controller
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

        // Lấy tất cả sản phẩm cho trang chủ
        private List<SanPhamViewModel> GetAllProducts()
        {
            var list = new List<SanPhamViewModel>();

            string connStr = GetConnStr();

            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("sp_SanPham_GetAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new SanPhamViewModel
                        {
                            MASP = rd.GetInt32(rd.GetOrdinal("MASP")),
                            TENSP = rd.GetString(rd.GetOrdinal("TENSP")),
                            GIABAN = rd.GetDecimal(rd.GetOrdinal("GIABAN")),
                            ANH = rd.IsDBNull(rd.GetOrdinal("ANH"))
                                        ? null
                                        : rd.GetString(rd.GetOrdinal("ANH"))
                        });
                    }
                }
            }
            return list;
        }
        // Trang chủ
        public ActionResult Index()
        {
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" }
            };

            var model = GetAllProducts();
            return View(model);
        }

        // PARTIAL: menu loại sản phẩm dùng foreach
        public PartialViewResult MenuLoai()
        {
            var list = new List<LoaiSP>();
            string connStr = GetConnStr();


            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("SELECT MALOAI, TENLOAI FROM LOAISP", conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new LoaiSP
                        {
                            MALOAI = (int)rd["MALOAI"],
                            TENLOAI = rd["TENLOAI"].ToString()
                        });
                    }
                }
            }

            return PartialView("_MenuLoai", list);
        }

        // Duyệt sản phẩm theo loại
        public ActionResult Duyet(int maloai)
        {
            var list = new List<SanPhamViewModel>();
            string connStr = ConfigurationManager.ConnectionStrings["Conn_Khach"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("SP_DuyetSanPham_TheoLoai", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaLoai", maloai);

                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new SanPhamViewModel
                        {
                            MASP = (int)rd["MASP"],
                            TENSP = rd["TENSP"].ToString(),
                            GIABAN = (decimal)rd["GIABAN"],
                            ANH = rd["ANH"] == DBNull.Value ? null : rd["ANH"].ToString(),
                            MALOAI = (int)rd["MALOAI"]
                        });
                    }
                }
            }

            // Thiết lập breadcrumb có link tới Trang chủ và link tới trang Duyet (theo loại)
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Duyệt theo loại", Action = "Duyet", Controller = "Home", RouteValues = new { maloai = maloai } }
            };

            ViewBag.MaLoai = maloai;
            return View(list);
        }

        //loc sp
        public ActionResult LocSanPham(int? maloai, string hang, decimal? giaMin, decimal? giaMax)
        {
            var list = new List<SanPhamViewModel>();

            string connStr = GetConnStr();


            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("SP_LocSanPham_Cursor", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // 1. Gửi đúng kiểu + cho phép NULL
                var pMaLoai = cmd.Parameters.Add("@MaLoai", SqlDbType.Int);
                pMaLoai.Value = (object)maloai ?? DBNull.Value;

                var pHang = cmd.Parameters.Add("@Hang", SqlDbType.VarChar, 50);
                pHang.Value = string.IsNullOrWhiteSpace(hang) ? (object)DBNull.Value : hang;
    
                var pGiaMin = cmd.Parameters.Add("@GiaMin", SqlDbType.Decimal);
                pGiaMin.Precision = 18;
                pGiaMin.Scale = 2;
                pGiaMin.Value = (object)giaMin ?? DBNull.Value;

                var pGiaMax = cmd.Parameters.Add("@GiaMax", SqlDbType.Decimal);
                pGiaMax.Precision = 18;
                pGiaMax.Scale = 2;
                pGiaMax.Value = (object)giaMax ?? DBNull.Value;

                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new SanPhamViewModel
                        {
                            MASP = (int)rd["MASP"],
                            TENSP = rd["TENSP"].ToString(),
                            GIABAN = (decimal)rd["GIABAN"],
                            ANH = rd["ANH"] == DBNull.Value ? null : rd["ANH"].ToString(),
                            MALOAI = (int)rd["MALOAI"]
                        });
                    }
                }
            }

            // 2. Giữ lại giá trị filter cho view
            ViewBag.MaLoai = maloai;
            ViewBag.Hang = hang;
            ViewBag.GiaMin = giaMin;
            ViewBag.GiaMax = giaMax;

            ViewBag.Breadcrumb = "Trang chủ / Lọc sản phẩm";

            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Lọc sản phẩm" }
            };

            return View("Duyet", list);
        }

        //tìm kiếm ở thanh tìm kiếm (theo loại, theo hàng hoặc theo tên sản phẩm)
        [HttpGet]
        public ActionResult TimKiem(string q)
        {
            var list = new List<SanPhamViewModel>();
            string connStr = GetConnStr();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("sp_SanPham_Search", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                // tìm kiếm
                var pQ = cmd.Parameters.Add("@q", SqlDbType.NVarChar, 200);
                pQ.Value = string.IsNullOrWhiteSpace(q) ? (object)DBNull.Value : q.Trim();
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new SanPhamViewModel
                        {
                            MASP = (int)rd["MASP"],
                            TENSP = rd["TENSP"].ToString(),
                            GIABAN = (decimal)rd["GIABAN"],
                            ANH = rd["ANH"] == DBNull.Value ? null : rd["ANH"].ToString(),
                            MALOAI = rd["MALOAI"] == DBNull.Value ? 0 : (int)rd["MALOAI"]
                        });
                    }
                }
            }
            //Giữ keyword để view hiển thị / giữ lại ô search
            ViewBag.Keyword = q;
            //Nếu view Duyet đang có form lọc, set rỗng để không trùng filter cũ
            ViewBag.MaLoai = null;
            ViewBag.Hang = null;
            ViewBag.GiaMin = null;
            ViewBag.GiaMax = null;
            ViewBag.Breadcrumb = "Trang chủ / Tìm kiếm";
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
    {
        new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
        new WebDienThoai.Models.BreadcrumbItem { Text = "Tìm kiếm" }
    };

            return View("Duyet", list);
        }


        //Xem chi tiết sản phẩm
        private XemChiTietModel GetProductDetail(int id)
        {
            XemChiTietModel result = null;
            string connStr = GetConnStr();


            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("SELECT * FROM dbo.fn_SanPham_ChiTiet(@MASP)", conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@MASP", id);

                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        result = new XemChiTietModel
                        {
                            MASP = (int)rd["MASP"],
                            TENSP = rd["TENSP"].ToString(),
                            GIABAN = rd["GIABAN"] != DBNull.Value
                                     ? (decimal)rd["GIABAN"] : 0,
                            ANH = rd["ANH"].ToString(),
                            KICHTHUOC = rd["KICHTHUOC"] != DBNull.Value
                                ? Convert.ToDouble(rd["KICHTHUOC"])
                                : (double?)null,
                            CONGNGHE = rd["CONGNGHE"].ToString(),
                            CAMERA = rd["CAMERA"].ToString(),
                            BONHO = rd["BONHO"] != DBNull.Value
                            ? Convert.ToInt32(rd["BONHO"])
                            : (int?)null
                        };
                    }
                }
            }
            return result;
        }

        public ActionResult ChiTietSanPham(int id)
        {
            var model = GetProductDetail(id);
            if (model == null)
                return HttpNotFound();

            // Breadcrumb: Trang chủ / Chi tiet san pham (hiển thị tên sp, không link)
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
                new WebDienThoai.Models.BreadcrumbItem { Text = model.TENSP } // không link, chỉ hiển thị
            };

            return View(model);
        }

        public ActionResult QuanTri()
        {
            var role = (Session["Role"] as string) ?? "KHACH";
            var isNhanVien = Session["IsNhanVien"] != null && Convert.ToBoolean(Session["IsNhanVien"]);

            if (!isNhanVien || role == "KHACH")
                return RedirectToAction("Index", "Home"); // chặn khách

            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
    {
        new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
        new WebDienThoai.Models.BreadcrumbItem { Text = "Quản trị" }
    };

            return View(); // Views/Home/QuanTri.cshtml
        }



    }
}
