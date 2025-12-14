using System;
using System.IO;
using System.Text;
using System.Web.Mvc;

public class PaymentController : Controller
{
    // 1) MoMo redirect browser về đây (GET)
    [HttpGet]
    public ActionResult MomoReturn()
    {
        // để test: xem query string MoMo trả về
        return Content("MomoReturn OK. Query=" + Request.QueryString.ToString());
    }

    // 2) MoMo server gọi IPN về đây (POST)
    [HttpPost]
    public ActionResult MomoIPN()
    {
        // để test: đọc raw body xem MoMo gửi gì
        string raw;
        using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
            raw = reader.ReadToEnd();

        // Bạn có thể log raw ra file/DB sau, hiện tại cứ trả 204 để MoMo biết đã nhận
        return new HttpStatusCodeResult(204);
    }
}
