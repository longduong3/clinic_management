using BELibrary.Core.Entity;
using BELibrary.Core.Utils;
using BELibrary.DbContext;
using BELibrary.Entity;
using HospitalManagement.HandelPayment;
using HospitalManagement.Handler;
using HospitalManagement.Handler.Payment.Momo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static BELibrary.Models.Constants;

namespace HospitalManagement.Controllers
{
    public class HomeController : Controller
    {
        string partnerCode = "MOMO5RGX20191128";
        string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create"; // API tạo cái QR momo trên site test của momo
        string accessKey = "M8brj9K6E22vXoDB";
        string serectkey = "nqQiVSgDMy809JoPF6OzP5OdBUB550Y4";
        string redirectUrl = "http://localhost:6688/home/MomoPaymentDone"; // Sau khi thanh toán
        string ipnUrl = "/payment-verify"; // API xác nhận đã thanh toán
        string requestType = "captureWallet";

        public ActionResult Index()
        {
            //var test = dbContext.News.FirstOrDefault();
            //var testview = Mapper.Map<NewsViewModel>(test);

            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                ViewBag.Doctors = workScope.Doctors.Include(x => x.Faculty).Take(8).ToList();

                var latestPosts = workScope.Articles.Query(x => !x.IsDelete).OrderByDescending(x => x.ModifiedDate).Take(5).ToList();
                ViewBag.LatestPosts = latestPosts;
            }

            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page. ";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page. ";

            return View();
        }

        public ActionResult E404()
        {
            ViewBag.Message = "Your contact page. ";

            return View();
        }

        [HttpPost]
        public JsonResult GetPaymentMomoUrl(Guid doctorId, DateTime time, FrameTimeEnum frameTime)
        {
            var user = CookiesManage.GetUser();

            if (user == null)
            {
                return Json(new { status = false, mess = "Vui lòng đăng nhập" });
            }

            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var isExist = workScope.DoctorSchedules.Query(x => x.DoctorId == doctorId && x.ScheduleBook.Year == time.Year && x.ScheduleBook.Month == time.Month && x.ScheduleBook.Day == time.Day && x.FrameTime == frameTime).Any();
                if (isExist)
                {
                    return Json(new { status = false, mess = "Khung giờ này đã được đặt trước, hệ thống tự động đề xuất bác sĩ khác!", isExist = true });
                }

                // Kiểm tra nếu thời gian đặt lịch trước thời điểm hiện tại
                if (!frameTime.ValidTime(time))
                {
                    return Json(new { status = false, mess = "Không thể đặt lịch trước thời gian hiện tại!" });
                }
            }

            string orderInfo = $"{doctorId},{time},{(int)frameTime},{user.PatientId}";
            string amount = 150000.ToString(); //Số tiền cần thanh toán
            string orderId = Guid.NewGuid().ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + ipnUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType
                ;

            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.SignSha256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "partnerName", "Test" },
                { "storeId", "MomoTestStore" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", redirectUrl },
                { "ipnUrl", ipnUrl },
                { "lang", "en" },
                { "extraData", extraData },
                { "requestType", requestType },
                { "signature", signature }

            };
            string responseFromMomo = PaymentRequest.SendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);

            return Json(new { status = true, url = jmessage.GetValue("payUrl").ToString() }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult MomoPaymentDone(MomoPaymentResult request)
        {
            if (string.IsNullOrEmpty(request.payType))
                return RedirectToAction("Index");

            if (request.partnerCode != partnerCode)
                throw new Exception("sssss"); // Kiểm tra thông tin

            var info = request.orderInfo.Split(',');
            var doctorId = Guid.Parse(info[0]);
            var time = DateTime.Parse(info[1]);
            var frameTime = (FrameTimeEnum)Int32.Parse(info[2]);
            Guid? patientId = null;
            if (!string.IsNullOrEmpty(info[3]))
                patientId = Guid.Parse(info[3]);

            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                // check Khung giờ bác sỹ rảnh
                var isExist = workScope.DoctorSchedules.Query(x => x.DoctorId == doctorId && x.ScheduleBook.Year == time.Year && x.ScheduleBook.Month == time.Month && x.ScheduleBook.Day == time.Day && x.FrameTime == frameTime).Any();
                if (isExist)
                {
                    return RedirectToAction("Index", "Account");
                }

                var doctor = workScope.Doctors.FirstOrDefault(x => x.Id == doctorId && !x.IsDelete);
                if (doctor == null)
                    return RedirectToAction("Index", "Account");

                workScope.DoctorSchedules.Add(new DoctorSchedule
                {
                    DoctorId = doctor.Id,
                    PatientId = patientId.GetValueOrDefault(),
                    ScheduleBook = time,
                    Status = BookingStatusKey.Pending,
                    FrameTime = frameTime,
                });
                workScope.Complete();

                var patientDoctor = workScope.PatientDoctors.FirstOrDefault(x =>
                    x.DoctorId == doctor.Id && x.PatientId == patientId);

                if (patientDoctor != null)
                    return RedirectToAction("Index", "Account");
                workScope.PatientDoctors.Add(new PatientDoctor
                {
                    PatientId = patientId.GetValueOrDefault(),
                    DoctorId = doctor.Id,
                    Status = 1
                });
                workScope.Complete();

                return RedirectToAction(nameof(Index));
            }
        }
    }

    public class MomoPaymentResult
    {
        public string partnerCode { get; set; }
        public string orderId { get; set; }
        public string requestId { get; set; }
        public decimal amount { get; set; }
        public string orderInfo { get; set; } // Cái doctorId đây
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