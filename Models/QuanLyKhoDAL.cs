using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using WebDienThoai.Models;
using WebDienThoai.Models.ViewModels;

namespace WebDienThoai.DAL
{
    public class QuanLyKhoDAL
    {
        private readonly string _cs;
        public QuanLyKhoDAL(string connectionString)
        {
            _cs = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        // ===== KHO =====
        public List<Kho> GetKhoAll()
        {
            var list = new List<Kho>();
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"SELECT MAKHO, TENKHO, DIACHI FROM dbo.KHO ORDER BY MAKHO", conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new Kho
                        {
                            MAKHO = rd["MAKHO"].ToString(),
                            TENKHO = rd["TENKHO"].ToString(),
                            DIACHI = rd["DIACHI"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // ===== TONKHO =====
        public List<TonKho> GetTonKho(string maKho = null)
        {
            var list = new List<TonKho>();
            var sql = @"SELECT MASP, MAKHO, SOLUONG FROM dbo.TONKHO
                        WHERE (@MAKHO IS NULL OR MAKHO = @MAKHO)
                        ORDER BY MAKHO, MASP";

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@MAKHO", (object)maKho ?? DBNull.Value);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new TonKho
                        {
                            MASP = rd["MASP"].ToString(),
                            MAKHO = rd["MAKHO"].ToString(),
                            SOLUONG = Convert.ToInt32(rd["SOLUONG"])
                        });
                    }
                }
            }
            return list;
        }

        // ===== PHIEUNHAP list =====
        public List<PhieuNhap> GetPhieuNhap(string maKho, DateTime? tuNgay, DateTime? denNgay)
        {
            var list = new List<PhieuNhap>();

            var sql = @"
                        SELECT pn.MAPHIEUNHAP, pn.ID, pn.NGAYNHAP, pn.NHACUNGCAP, pn.TONGGIA, pn.MAKHO,
                               k.TENKHO
                        FROM dbo.PHIEUNHAP pn
                        LEFT JOIN dbo.KHO k ON k.MAKHO = pn.MAKHO
                        WHERE (@MAKHO IS NULL OR pn.MAKHO = @MAKHO)
                          AND (@TUNGAY IS NULL OR pn.NGAYNHAP >= @TUNGAY)
                          AND (@DENNGAY IS NULL OR pn.NGAYNHAP < DATEADD(DAY, 1, @DENNGAY))
                        ORDER BY pn.NGAYNHAP DESC, pn.MAPHIEUNHAP DESC";

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@MAKHO", (object)maKho ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TUNGAY", (object)tuNgay ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DENNGAY", (object)denNgay ?? DBNull.Value);

                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new PhieuNhap
                        {
                            MAPHIEUNHAP = Convert.ToInt32(rd["MAPHIEUNHAP"]),
                            ID = rd["ID"] as string,
                            NGAYNHAP = Convert.ToDateTime(rd["NGAYNHAP"]),
                            NHACUNGCAP = rd["NHACUNGCAP"].ToString(),
                            TONGGIA = Convert.ToDecimal(rd["TONGGIA"]),
                            MAKHO = rd["MAKHO"].ToString(),
                            TENKHO = rd["TENKHO"]?.ToString()
                        });
                    }
                }
            }

            return list;
        }

        public PhieuNhap GetPhieuNhapById(int id)
        {
            var sql = @"
                        SELECT pn.MAPHIEUNHAP, pn.ID, pn.NGAYNHAP, pn.NHACUNGCAP, pn.TONGGIA, pn.MAKHO,
                               k.TENKHO
                        FROM dbo.PHIEUNHAP pn
                        LEFT JOIN dbo.KHO k ON k.MAKHO = pn.MAKHO
                        WHERE pn.MAPHIEUNHAP = @ID";

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;

                    return new PhieuNhap
                    {
                        MAPHIEUNHAP = Convert.ToInt32(rd["MAPHIEUNHAP"]),
                        ID = rd["ID"] as string,
                        NGAYNHAP = Convert.ToDateTime(rd["NGAYNHAP"]),
                        NHACUNGCAP = rd["NHACUNGCAP"].ToString(),
                        TONGGIA = Convert.ToDecimal(rd["TONGGIA"]),
                        MAKHO = rd["MAKHO"].ToString(),
                        TENKHO = rd["TENKHO"]?.ToString()
                    };
                }
            }
        }

        public List<ChiTietPN> GetChiTietPN(int maPhieuNhap)
        {
            var list = new List<ChiTietPN>();
            var sql = @"SELECT MAPHIEUNHAP, MASP, SOLUONG, GIANHAP
                        FROM dbo.CHITIETPN
                        WHERE MAPHIEUNHAP = @ID
                        ORDER BY MASP";

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", maPhieuNhap);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new ChiTietPN
                        {
                            MAPHIEUNHAP = Convert.ToInt32(rd["MAPHIEUNHAP"]),
                            MASP = rd["MASP"].ToString(),
                            SOLUONG = Convert.ToInt32(rd["SOLUONG"]),
                            GIANHAP = Convert.ToDecimal(rd["GIANHAP"])
                        });
                    }
                }
            }
            return list;
        }

        // đổi signature: idNguoiTao int? , maKho int
        public int CreatePhieuNhap(int idNguoiTao, DateTime ngayNhap, string nhaCungCap, int maKho, List<ChiTietPN> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Items rỗng.");

            if (string.IsNullOrWhiteSpace(nhaCungCap))
                throw new ArgumentException("Nhà cung cấp rỗng.");

            decimal tongGia = 0m;
            foreach (var it in items)
                tongGia += (decimal)it.SOLUONG * it.GIANHAP;

            using (var conn = new SqlConnection(_cs))
            {
                conn.Open();

                // đảm bảo rollback khi gặp lỗi SQL
                using (var cmdXact = new SqlCommand("SET XACT_ABORT ON;", conn))
                    cmdXact.ExecuteNonQuery();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) insert PHIEUNHAP
                        const string insertPN = @"
                                                INSERT INTO dbo.PHIEUNHAP (ID, NGAYNHAP, NHACUNGCAP, TONGGIA, MAKHO)
                                                VALUES (@ID, @NGAYNHAP, @NCC, @TONGGIA, @MAKHO);
                                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int newId;
                        using (var cmd = new SqlCommand(insertPN, conn, tx))
                        {
                            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = idNguoiTao;
                            cmd.Parameters.Add("@NGAYNHAP", SqlDbType.DateTime).Value = ngayNhap;

                            cmd.Parameters.Add("@NCC", SqlDbType.NVarChar, 100).Value = nhaCungCap.Trim();

                            var pTong = cmd.Parameters.Add("@TONGGIA", SqlDbType.Decimal);
                            pTong.Precision = 18;
                            pTong.Scale = 2;
                            pTong.Value = tongGia;

                            cmd.Parameters.Add("@MAKHO", SqlDbType.Int).Value = maKho;

                            newId = (int)cmd.ExecuteScalar();
                        }

                        // 2) insert CHITIETPN + 3) upsert TONKHO
                        const string insertCT = @"
INSERT INTO dbo.CHITIETPN (MAPHIEUNHAP, MASP, SOLUONG, GIANHAP)
VALUES (@MAPN, @MASP, @SL, @GIA);";

                        const string upsertTon = @"
IF EXISTS (SELECT 1 FROM dbo.TONKHO WHERE MAKHO = @MAKHO AND MASP = @MASP)
    UPDATE dbo.TONKHO SET SOLUONG = SOLUONG + @SL WHERE MAKHO = @MAKHO AND MASP = @MASP;
ELSE
    INSERT INTO dbo.TONKHO (MASP, MAKHO, SOLUONG) VALUES (@MASP, @MAKHO, @SL);";

                        foreach (var it in items)
                        {
                            if (it == null) continue;

                            if (!int.TryParse(it.MASP?.ToString(), out int maspInt))
                                throw new FormatException($"MASP không hợp lệ: '{it?.MASP}' (phải là số).");

                            if (it.SOLUONG <= 0)
                                throw new ArgumentException($"Số lượng không hợp lệ cho MASP {maspInt}.");

                            if (it.GIANHAP <= 0)
                                throw new ArgumentException($"Giá nhập không hợp lệ cho MASP {maspInt}.");

                            using (var cmd = new SqlCommand(insertCT, conn, tx))
                            {
                                cmd.Parameters.Add("@MAPN", SqlDbType.Int).Value = newId;
                                cmd.Parameters.Add("@MASP", SqlDbType.Int).Value = maspInt;
                                cmd.Parameters.Add("@SL", SqlDbType.Int).Value = it.SOLUONG;

                                var pGia = cmd.Parameters.Add("@GIA", SqlDbType.Decimal);
                                pGia.Precision = 18;
                                pGia.Scale = 2;
                                pGia.Value = it.GIANHAP;

                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand(upsertTon, conn, tx))
                            {
                                cmd.Parameters.Add("@MAKHO", SqlDbType.Int).Value = maKho;
                                cmd.Parameters.Add("@MASP", SqlDbType.Int).Value = maspInt;
                                cmd.Parameters.Add("@SL", SqlDbType.Int).Value = it.SOLUONG;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return newId;
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { /* ignore rollback errors */ }
                        throw;
                    }
                }
            }
        }
        public List<DonHangKhoItemVM> DonHangKho_List(int? maHD, DateTime? tuNgay, DateTime? denNgay, string trangThai)
        {
            var list = new List<DonHangKhoItemVM>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("dbo.usp_DonHangKho_List", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MAHD", (object)maHD ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TUNGAY", (object)tuNgay ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DENNGAY", (object)denNgay ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TRANGTHAI", string.IsNullOrWhiteSpace(trangThai) ? (object)DBNull.Value : trangThai);

                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new DonHangKhoItemVM
                        {
                            MaHD = Convert.ToInt32(rd["MAHD"]),
                            NgayLap = Convert.ToDateTime(rd["NGAYLAP"]),
                            ThanhTien = Convert.ToDecimal(rd["THANHTIEN"]),
                            PhuongThucThanhToan = rd["PHUONGTHUCTHANHTOAN"]?.ToString(),
                            HoTen = rd["HOTEN"]?.ToString(),
                            SDT = rd["SDT"]?.ToString(),
                            DiaChi = rd["DIACHI"]?.ToString(),
                            TrangThaiGiaoHang = rd["TRANGTHAI"]?.ToString(),
                            NgayGiao = rd["NGAYGIAO"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["NGAYGIAO"])
                        });
                    }
                }
            }
            return list;
        }

        public void DonHangKho_UpdateTrangThai(int maHD, string trangThaiMoi)
        {
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("dbo.usp_DonHangKho_UpdateTrangThai", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@MAHD", SqlDbType.Int).Value = maHD;
                cmd.Parameters.Add("@TRANGTHAI_MOI", SqlDbType.NVarChar, 50).Value = trangThaiMoi;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}