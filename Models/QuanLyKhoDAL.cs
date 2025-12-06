using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using WebDienThoai.Models;

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

        // ===== CREATE PHIEU NHAP (Transaction + update TONKHO) =====
        public int CreatePhieuNhap(string idNguoiTao, DateTime ngayNhap, string nhaCungCap, string maKho, List<ChiTietPN> items)
        {
            if (items == null || items.Count == 0) throw new ArgumentException("Items rỗng.");

            // tính tổng
            decimal tongGia = 0;
            foreach (var it in items) tongGia += (decimal)it.SOLUONG * it.GIANHAP;

            using (var conn = new SqlConnection(_cs))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) insert PHIEUNHAP lấy identity
                        var insertPN = @"
                                        INSERT INTO dbo.PHIEUNHAP (ID, NGAYNHAP, NHACUNGCAP, TONGGIA, MAKHO)
                                        VALUES (@ID, @NGAYNHAP, @NCC, @TONGGIA, @MAKHO);
                                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int newId;
                        using (var cmd = new SqlCommand(insertPN, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ID", (object)idNguoiTao ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@NGAYNHAP", ngayNhap);
                            cmd.Parameters.AddWithValue("@NCC", nhaCungCap);
                            cmd.Parameters.AddWithValue("@TONGGIA", tongGia);
                            cmd.Parameters.AddWithValue("@MAKHO", maKho);
                            newId = (int)cmd.ExecuteScalar();
                        }

                        // 2) insert CHITIETPN + 3) upsert TONKHO
                        foreach (var it in items)
                        {
                            var insertCT = @"INSERT INTO dbo.CHITIETPN (MAPHIEUNHAP, MASP, SOLUONG, GIANHAP)
                                             VALUES (@MAPN, @MASP, @SL, @GIA)";
                            using (var cmd = new SqlCommand(insertCT, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@MAPN", newId);
                                cmd.Parameters.AddWithValue("@MASP", it.MASP);
                                cmd.Parameters.AddWithValue("@SL", it.SOLUONG);
                                cmd.Parameters.AddWithValue("@GIA", it.GIANHAP);
                                cmd.ExecuteNonQuery();
                            }

                            // upsert TONKHO
                            var upsertTon = @"
                                            IF EXISTS (SELECT 1 FROM dbo.TONKHO WHERE MAKHO = @MAKHO AND MASP = @MASP)
                                                UPDATE dbo.TONKHO SET SOLUONG = SOLUONG + @SL WHERE MAKHO = @MAKHO AND MASP = @MASP;
                                            ELSE
                                                INSERT INTO dbo.TONKHO (MASP, MAKHO, SOLUONG) VALUES (@MASP, @MAKHO, @SL);";

                            using (var cmd = new SqlCommand(upsertTon, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@MAKHO", maKho);
                                cmd.Parameters.AddWithValue("@MASP", it.MASP);
                                cmd.Parameters.AddWithValue("@SL", it.SOLUONG);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return newId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
