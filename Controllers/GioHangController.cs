using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using WebDienThoai.Models;

namespace WebDienThoai.Controllers
{
    public class GioHangController : Controller
    {
        private string GetConnStr()
        {
            var cs = ConfigurationManager.ConnectionStrings["Conn_Khach"];
            return cs?.ConnectionString;
        }
        public ActionResult Index()
        {
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Giỏ hàng" } // không link
            };

                var cart = Session["GioHang"] as List<GioHang> ?? new List<GioHang>();
            ViewBag.TongSanPham = cart.Sum(i => i.SOLUONG);
            ViewBag.TongTien = cart.Sum(i => i.ThanhTien);
            return View(cart);
        }

        public ActionResult Add(int id, int qty = 1)
        {
            if (qty < 1) qty = 1;
            var cart = Session["GioHang"] as List<GioHang> ?? new List<GioHang>();

            var existing = cart.FirstOrDefault(x => x.MASP == id);
            if (existing != null)
            {
                existing.SOLUONG += qty;
            }
            else
            {
                var p = GetProductDetail(id);
                if (p == null)
                    return HttpNotFound();

                cart.Add(new GioHang
                {
                    MASP = p.MASP,
                    TENSP = p.TENSP,
                    DONGIA = p.GIABAN,
                    ANH = p.ANH,
                    SOLUONG = qty
                });
            }

            Session["GioHang"] = cart;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Update(int masp, int soluong)
        {
            var cart = Session["GioHang"] as List<GioHang>;
            if (cart != null)
            {
                var item = cart.FirstOrDefault(x => x.MASP == masp);
                if (item != null)
                {
                    if (soluong <= 0)
                        cart.Remove(item);
                    else
                        item.SOLUONG = soluong;
                    Session["GioHang"] = cart;
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Remove(int id)
        {
            var cart = Session["GioHang"] as List<GioHang>;
            if (cart != null)
            {
                var itm = cart.FirstOrDefault(x => x.MASP == id);
                if (itm != null)
                {
                    cart.Remove(itm);
                    Session["GioHang"] = cart;
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Clear()
        {
            Session.Remove("GioHang");
            return RedirectToAction("Index");
        }

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
                            GIABAN = rd["GIABAN"] != DBNull.Value ? (decimal)rd["GIABAN"] : 0,
                            ANH = rd["ANH"] == DBNull.Value ? null : rd["ANH"].ToString()
                        };
                    }
                }
            }
            return result;
        }
    }
}