using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Data.SqlClient;
using System.Web.Mvc;
using WebDienThoai.DAL;
using WebDienThoai.Models;
using WebDienThoai.Models.ViewModels;

namespace WebDienThoai.Controllers
{
    public class QuanLyKhoController : Controller
    {
        private QuanLyKhoDAL _dal;

        private string GetConnStr()
        {
            // Quản lý kho = nghiệp vụ admin => default Conn_Admin
            var connName = Session["ConnName"] as string;
            if (string.IsNullOrWhiteSpace(connName))
                connName = "Conn_Admin";

            var cs = ConfigurationManager.ConnectionStrings[connName]
                  ?? ConfigurationManager.ConnectionStrings["Conn_Admin"];

            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                throw new InvalidOperationException("Thiếu connection string 'Conn_Admin' (hoặc ConnName) trong Web.config.");

            return cs.ConnectionString;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _dal = new QuanLyKhoDAL(GetConnStr());
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index()
        {
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Quản lý kho" }
            };

            var kho = _dal.GetKhoAll();
            return View(kho);
        }

        public ActionResult TonKho(string maKho)
        {
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Quản lý kho", Action = "Index", Controller = "QuanLyKho" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Tồn kho" }
            };

            var khoList = _dal.GetKhoAll();
            var vm = new TonKhoVM
            {
                MAKHO = maKho,
                KhoList = khoList,
                Items = _dal.GetTonKho(maKho)
            };

            if (!string.IsNullOrWhiteSpace(maKho))
                vm.TENKHO = khoList.FirstOrDefault(x => x.MAKHO == maKho)?.TENKHO;

            return View(vm);
        }

        public ActionResult PhieuNhap(string maKho, DateTime? tuNgay, DateTime? denNgay)
        {
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Quản lý kho", Action = "Index", Controller = "QuanLyKho" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Phiếu nhập" }
            };

            var vm = new PhieuNhapListVM
            {
                MAKHO = maKho,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                KhoList = _dal.GetKhoAll(),
                Items = _dal.GetPhieuNhap(maKho, tuNgay, denNgay)
            };

            return View(vm);
        }

        public ActionResult ChiTietPhieuNhap(int id)
        {
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Quản lý kho", Action = "Index", Controller = "QuanLyKho" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Chi tiết phiếu nhập" }
            };

            var pn = _dal.GetPhieuNhapById(id);
            if (pn == null) return HttpNotFound();

            var vm = new PhieuNhapDetailVM
            {
                PhieuNhap = pn,
                ChiTiet = _dal.GetChiTietPN(id)
            };
            return View(vm);
        }

        [HttpGet]
        public ActionResult TaoPhieuNhap()
        {
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Quản lý kho", Action = "Index", Controller = "QuanLyKho" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Tạo phiếu nhập" }
            };

            var vm = new CreatePhieuNhapVM
            {
                NGAYNHAP = DateTime.Now,
                KhoList = _dal.GetKhoAll()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaoPhieuNhap(CreatePhieuNhapVM vm)
        {
            // luôn đổ lại KhoList để View không lỗi khi return
            vm.KhoList = _dal.GetKhoAll();

            // ===== validate form =====
            if (string.IsNullOrWhiteSpace(vm.MAKHO))
                ModelState.AddModelError("MAKHO", "Vui lòng chọn kho.");

            if (string.IsNullOrWhiteSpace(vm.NHACUNGCAP))
                ModelState.AddModelError("NHACUNGCAP", "Vui lòng nhập nhà cung cấp.");

            vm.Items = (vm.Items ?? new List<CreatePhieuNhapItemVM>())
                        .Where(x => x != null)
                        .ToList();

            if (vm.Items.Count == 0)
                ModelState.AddModelError("", "Cần ít nhất 1 dòng sản phẩm.");

            for (int i = 0; i < vm.Items.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(vm.Items[i].MASP))
                    ModelState.AddModelError($"Items[{i}].MASP", "Vui lòng nhập MASP.");

                if (vm.Items[i].SOLUONG <= 0)
                    ModelState.AddModelError($"Items[{i}].SOLUONG", "Số lượng phải >= 1.");

                if (vm.Items[i].GIANHAP <= 0)
                    ModelState.AddModelError($"Items[{i}].GIANHAP", "Giá nhập phải > 0.");
            }

            // ===== bắt buộc đăng nhập vì PHIEUNHAP.ID NOT NULL =====
            if (Session["UserId"] == null)
            {
                ModelState.AddModelError("", "Bạn cần đăng nhập (có ID người tạo) để tạo phiếu nhập.");
                return View(vm);
            }

            if (!ModelState.IsValid) return View(vm);

            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);   // INT
                int maKhoInt = Convert.ToInt32(vm.MAKHO);          // INT
                DateTime ngayNhap = vm.NGAYNHAP ?? DateTime.Now;   // FIX: thiếu biến này trong code bạn

                // Giữ MASP dạng string, DAL sẽ Convert.ToInt32 để insert
                var items = vm.Items.Select(x => new ChiTietPN
                {
                    MASP = x.MASP.Trim(),
                    SOLUONG = x.SOLUONG,
                    GIANHAP = x.GIANHAP
                }).ToList();

                int newId = _dal.CreatePhieuNhap(
                    userId,
                    ngayNhap,
                    vm.NHACUNGCAP.Trim(),
                    maKhoInt,
                    items
                );

                TempData["Success"] = "Tạo phiếu nhập thành công.";
                return RedirectToAction("ChiTietPhieuNhap", new { id = newId });
            }
            catch (FormatException)
            {
                ModelState.AddModelError("", "MASP/MAKHO phải là số (int). Vui lòng kiểm tra lại.");
                return View(vm);
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", "Có lỗi khi tạo phiếu nhập (SQL): " + ex.Message);
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi khi tạo phiếu nhập: " + ex.Message);
                return View(vm);
            }
        }
    }
}
