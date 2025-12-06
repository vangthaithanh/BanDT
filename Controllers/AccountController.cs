using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using WebDienThoai.Models;

namespace WebDienThoai.Controllers
{
    public class AccountController : Controller
    {
        // Kết nối dùng riêng cho LOGIN
        private readonly string _connLogin =
            ConfigurationManager.ConnectionStrings["Conn_Admin"].ConnectionString;

        // Hàm trợ giúp để các chỗ khác dùng connection string theo role
        internal static string GetConnectionStringForCurrentUser(System.Web.SessionState.HttpSessionState session)
        {
            var name = session?["ConnName"] as string;
            if (string.IsNullOrEmpty(name))
                name = "Conn_Khach"; // mặc định là khách

            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.Breadcrumb = "Đăng nhập";
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using (var conn = new SqlConnection(_connLogin)) // LUÔN DÙNG CONN_ADMIN ĐỂ LOGIN
                using (var cmd = new SqlCommand("usp_Account_Login_WithRole", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TENTK", model.UserName);
                    cmd.Parameters.AddWithValue("@MATKHAU", model.Password);

                    conn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            string tentk = rd["TENTK"].ToString();
                            int idNguoiDung = rd["ID"] != DBNull.Value
                                              ? Convert.ToInt32(rd["ID"])
                                              : 0;
                            string hoTen = rd["HOTEN"] != DBNull.Value
                                           ? rd["HOTEN"].ToString()
                                           : tentk;

                            bool isNhanVien = rd["IS_NHANVIEN"] != DBNull.Value
                                              && Convert.ToInt32(rd["IS_NHANVIEN"]) == 1;
                            string roleCode = rd["ROLE_CODE"] != DBNull.Value
                                              ? rd["ROLE_CODE"].ToString()
                                              : "KHACH";

                            // Lưu thông tin chung
                            Session["UserName"] = tentk;
                            Session["UserId"] = idNguoiDung;
                            Session["HoTen"] = hoTen;
                            Session["IsNhanVien"] = isNhanVien;
                            Session["Role"] = roleCode;

                            // Chọn connection string theo role
                            string connName;
                            if (!isNhanVien || roleCode == "KHACH")
                            {
                                connName = "Conn_Khach";
                            }
                            else
                            {
                                switch (roleCode)
                                {
                                    case "ADMIN":
                                        connName = "Conn_Admin";
                                        break;
                                    case "NVKHO":
                                        connName = "Conn_Kho";
                                        break;
                                    case "NVSP":
                                        connName = "Conn_SanPham";
                                        break;
                                    default:
                                        connName = "Conn_Khach";
                                        break;
                                }
                            }

                            Session["ConnName"] = connName;

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                            return View(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                return View(model);
            }
        }

        // GET: /Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            ViewBag.Breadcrumb = "Đăng ký";
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu nhập lại không khớp.");
                return View(model);
            }

            try
            {
                using (var conn = new SqlConnection(_connLogin))
                using (var cmd = new SqlCommand("usp_Account_Register", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@TENTK", model.UserName);
                    cmd.Parameters.AddWithValue("@MATKHAU", model.Password);

                    cmd.Parameters.AddWithValue("@HOTEN", (object)model.HoTen ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SDT", (object)model.SDT ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EMAIL", (object)model.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DIACHI", (object)model.DiaChi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GIOITINH", (object)model.GioiTinh ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DANTOC", (object)model.DanToc ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["RegisterSuccess"] = "Đăng ký thành công, vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (SqlException ex)
            {
                var msg = ex.Message;

                if (msg.Contains("Tên đăng nhập đã tồn tại"))
                {
                    ModelState.AddModelError("UserName", msg);
                }
                else if (msg.Contains("Số điện thoại này đã được sử dụng"))
                {
                    ModelState.AddModelError("SDT", msg);
                }
                else if (msg.Contains("Email này đã được sử dụng"))
                {
                    ModelState.AddModelError("Email", msg);
                }
                else if (msg.Contains("UQ_NGUOIDUNG_SDT"))
                {
                    // lỗi từ UNIQUE constraint SDT
                    ModelState.AddModelError("SDT", "Số điện thoại này đã được sử dụng, vui lòng nhập số khác.");
                }
                else if (msg.Contains("UQ_NGUOIDUNG_EMAIL")) // nếu bạn có unique cho email
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng, vui lòng nhập email khác.");
                }
                else
                {
                    ModelState.AddModelError("", "Dữ liệu không hợp lệ: " + msg);
                }

                return View(model);
            }


            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                return View(model);
            }
        }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }


    }
    
}
