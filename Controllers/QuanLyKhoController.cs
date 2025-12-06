using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebDienThoai.DAL;
using WebDienThoai.Models;
using WebDienThoai.Models.ViewModels;

namespace WebDienThoai.Controllers
{
    public class QuanLyKhoController : Controller
    {
        // GET: QuanLyKho
        private QuanLyKhoDAL _dal;

        private string GetConnStr()
        {
            var connName = Session["ConnName"] as string;
            if (string.IsNullOrWhiteSpace(connName))
                connName = "Conn_Khach";

            var cs = ConfigurationManager.ConnectionStrings[connName]
                  ?? ConfigurationManager.ConnectionStrings["Conn_Khach"];

            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                throw new InvalidOperationException("Thiếu connection string 'Conn_Khach' (hoặc ConnName) trong Web.config.");

            return cs.ConnectionString;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _dal = new QuanLyKhoDAL(GetConnStr());
            base.OnActionExecuting(filterContext);
        }

        // /QuanLyKho
        public ActionResult Index()
        {
            // breadcrumb
            ViewBag.BreadcrumbList = new List<WebDienThoai.Models.BreadcrumbItem>
            {
                new WebDienThoai.Models.BreadcrumbItem { Text = "Trang chủ", Action = "Index", Controller = "Home" },
                new WebDienThoai.Models.BreadcrumbItem { Text = "Quản lý kho" }
            };

            var kho = _dal.GetKhoAll();
            return View(kho);
        }

        // /QuanLyKho/TonKho?maKho=K001
        public ActionResult TonKho(string maKho)
        {
            // breadcrumb
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

        // /QuanLyKho/PhieuNhap?maKho=K001&tuNgay=2025-12-01&denNgay=2025-12-04
        public ActionResult PhieuNhap(string maKho, DateTime? tuNgay, DateTime? denNgay)
        {
            // breadcrumb
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

        // /QuanLyKho/ChiTietPhieuNhap/5
        public ActionResult ChiTietPhieuNhap(int id)
        {
            // breadcrumb
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
            // breadcrumb
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
            vm.KhoList = _dal.GetKhoAll();

            // validate nhanh
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

            if (!ModelState.IsValid) return View(vm);

            try
            {
                var items = vm.Items.Select(x => new ChiTietPN
                {
                    MASP = x.MASP.Trim(),
                    SOLUONG = x.SOLUONG,
                    GIANHAP = x.GIANHAP
                }).ToList();

                var ngayNhap = vm.NGAYNHAP ?? DateTime.Now;

                // nếu bạn có login: var userId = (string)Session["ID"];
                string userId = null;

                int newId = _dal.CreatePhieuNhap(
                    userId,
                    ngayNhap,
                    vm.NHACUNGCAP.Trim(),
                    vm.MAKHO,
                    items
                );

                return RedirectToAction("ChiTietPhieuNhap", new { id = newId });
            }
            catch
            {
                ModelState.AddModelError("", "Có lỗi khi tạo phiếu nhập. Vui lòng thử lại.");
                return View(vm);
            }
        }
    }
}
