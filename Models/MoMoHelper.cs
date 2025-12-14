using System;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebDienThoai.Helpers
{
    public class MoMoHelper
    {
        // =============================================
        // THÔNG TIN TÍCH HỢP MOMO (ĐỔI THÀNH THÔNG TIN CỦA BẠN)
        // =============================================
        private static readonly string PartnerCode = "MOMOBKUN20180529"; // Test partner code
        private static readonly string AccessKey = "klm05TvNBzhg7h7j"; // Test access key
        private static readonly string SecretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa"; // Test secret key

        // Endpoint TEST của MoMo
        private static readonly string MoMoEndpoint = "https://test-payment.momo.vn/v2/gateway/api/create";

        // URL callback - MoMo sẽ gọi lại URL này sau khi khách thanh toán
        private static readonly string ReturnUrl = "https://yourdomain.com/GioHang/MoMoReturn"; // Đổi thành domain của bạn
        private static readonly string NotifyUrl = "https://yourdomain.com/GioHang/MoMoNotify"; // Đổi thành domain của bạn

        /// <summary>
        /// Tạo yêu cầu thanh toán MoMo
        /// </summary>
        public static async Task<MoMoPaymentResponse> CreatePaymentAsync(
            int orderId,
            decimal amount,
            string orderInfo,
            string extraData = "")
        {
            try
            {
                var requestId = Guid.NewGuid().ToString();
                var orderIdStr = "DH" + orderId.ToString();
                var requestType = "captureWallet"; // Thanh toán qua ví MoMo

                // Tạo chữ ký (signature)
                var rawHash = $"accessKey={AccessKey}" +
                             $"&amount={amount}" +
                             $"&extraData={extraData}" +
                             $"&ipnUrl={NotifyUrl}" +
                             $"&orderId={orderIdStr}" +
                             $"&orderInfo={orderInfo}" +
                             $"&partnerCode={PartnerCode}" +
                             $"&redirectUrl={ReturnUrl}" +
                             $"&requestId={requestId}" +
                             $"&requestType={requestType}";

                var signature = ComputeHmacSha256(rawHash, SecretKey);

                // Tạo request body
                var requestData = new
                {
                    partnerCode = PartnerCode,
                    partnerName = "Shop Điện Thoại",
                    storeId = "MomoTestStore",
                    requestId = requestId,
                    amount = amount,
                    orderId = orderIdStr,
                    orderInfo = orderInfo,
                    redirectUrl = ReturnUrl,
                    ipnUrl = NotifyUrl,
                    lang = "vi",
                    extraData = extraData,
                    requestType = requestType,
                    signature = signature
                };

                // Gửi request đến MoMo
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    var jsonContent = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(MoMoEndpoint, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var momoResponse = JsonConvert.DeserializeObject<MoMoPaymentResponse>(responseContent);
                    return momoResponse;
                }
            }
            catch (Exception ex)
            {
                return new MoMoPaymentResponse
                {
                    resultCode = -1,
                    message = "Lỗi kết nối MoMo: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Xác thực callback từ MoMo
        /// </summary>
        public static bool VerifySignature(
            string partnerCode,
            string accessKey,
            string requestId,
            long amount,
            string orderId,
            string orderInfo,
            string orderType,
            long transId,
            int resultCode,
            string message,
            string payType,
            long responseTime,
            string extraData,
            string signature)
        {
            try
            {
                var rawHash = $"accessKey={accessKey}" +
                             $"&amount={amount}" +
                             $"&extraData={extraData}" +
                             $"&message={message}" +
                             $"&orderId={orderId}" +
                             $"&orderInfo={orderInfo}" +
                             $"&orderType={orderType}" +
                             $"&partnerCode={partnerCode}" +
                             $"&payType={payType}" +
                             $"&requestId={requestId}" +
                             $"&responseTime={responseTime}" +
                             $"&resultCode={resultCode}" +
                             $"&transId={transId}";

                var computedSignature = ComputeHmacSha256(rawHash, SecretKey);
                return signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tính toán HMAC SHA256
        /// </summary>
        private static string ComputeHmacSha256(string message, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }

    // =============================================
    // MODELS
    // =============================================

    /// <summary>
    /// Response từ MoMo API
    /// </summary>
    public class MoMoPaymentResponse
    {
        public string partnerCode { get; set; }
        public string requestId { get; set; }
        public string orderId { get; set; }
        public long amount { get; set; }
        public long responseTime { get; set; }
        public string message { get; set; }
        public int resultCode { get; set; }
        public string payUrl { get; set; } // URL để redirect khách hàng đến
        public string deeplink { get; set; }
        public string qrCodeUrl { get; set; }
    }

    /// <summary>
    /// Request callback từ MoMo
    /// </summary>
    public class MoMoCallbackRequest
    {
        public string partnerCode { get; set; }
        public string orderId { get; set; }
        public string requestId { get; set; }
        public long amount { get; set; }
        public string orderInfo { get; set; }
        public string orderType { get; set; }
        public long transId { get; set; }
        public int resultCode { get; set; }
        public string message { get; set; }
        public string payType { get; set; }
        public long responseTime { get; set; }
        public string extraData { get; set; }
        public string signature { get; set; }
    }
}