using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace WebDienThoai.Controllers
{
    public class BackupController : Controller
    {
        private string BackupFolder = @"F:\Backup\";

        private string GetConnStr(string connName = null)
        {
            if (connName == null)
            {
                var s = Session["ConnName"] as string;
                if (string.IsNullOrEmpty(s)) s = "Conn_Khach";
                connName = s;
            }

            var cs = ConfigurationManager.ConnectionStrings[connName];
            return cs.ConnectionString;
        }

        // Gọi PROC
        private void ExecuteProc(string procName, SqlParameter[] parameters, string connName = "Conn_Admin")
        {
            using (SqlConnection conn = new SqlConnection(GetConnStr(connName)))
            using (SqlCommand cmd = new SqlCommand(procName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ---------------- BACKUP FULL ----------------
        public ActionResult BackupFull()
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            string file = BackupFolder + $"Test_webdt_Full_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

            ExecuteProc(
                "usp_BackupFull",
                new[] { new SqlParameter("@FilePath", file) },
                "Conn_Admin"
            );

            TempData["msg"] = $"Đã tạo Full Backup: {file}";
            return RedirectToAction("Backup", "Account");
        }

        // ---------------- BACKUP DIFF ----------------
        public ActionResult BackupDiff()
        {
            string file = BackupFolder + $"Test_webdt_Diff_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

            ExecuteProc(
                "usp_BackupDiff",
                new[] { new SqlParameter("@FilePath", file) },
                "Conn_Admin"
            );

            TempData["msg"] = $"Đã tạo Differential Backup: {file}";
            return RedirectToAction("Backup", "Account");
        }

        // ---------------- BACKUP LOG ----------------
        public ActionResult BackupLog()
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            string file = BackupFolder + $"Test_webdt_Log_{DateTime.Now:yyyyMMdd_HHmmss}.trn";

            ExecuteProc(
                "usp_BackupLog",
                new[] { new SqlParameter("@FilePath", file) },
                "Conn_Admin"
            );

            TempData["msg"] = $"Đã tạo Log Backup: {file}";
            return RedirectToAction("Backup", "Account");
        }

        // Lấy file mới nhất theo pattern
        private string GetLatestBackupFile(string pattern)
        {
            var files = Directory.GetFiles(BackupFolder, pattern);

            if (files.Length == 0)
                throw new Exception("Không tìm thấy file backup!");

            return files
                .OrderByDescending(f => System.IO.File.GetLastWriteTime(f))
                .First();
        }

        // ---------------- RESTORE ----------------
        public ActionResult Restore()
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            // Lấy file FULL mới nhất
            string latestFull = GetLatestBackupFile("Test_webdt_Full_*.bak");

            ExecuteProc("usp_RestoreFull",
                        new[] { new SqlParameter("@File", latestFull) },
                        "Conn_Master"); // restore phải chạy trên master


            TempData["msg"] = "Khôi phục thành công từ Full Backup!";
            return RedirectToAction("Backup", "Account");
        }

        // ---------------- CHECK ADMIN ----------------
        private bool IsAdmin()
        {
            return Session["Role"] != null &&
                   Session["Role"].ToString() == "ADMIN";
        }
    }
}
