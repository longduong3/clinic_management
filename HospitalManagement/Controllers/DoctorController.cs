using BELibrary.Core.Entity;
using BELibrary.Core.Utils;
using BELibrary.DbContext;
using BELibrary.Entity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HospitalManagement.Handler;
using static BELibrary.Models.Constants;
using System.Data.Entity.Infrastructure;
using System.Web.WebPages.Html;

namespace HospitalManagement.Controllers
{
    public class DoctorController : Controller
    {
        // GET: Doctor
        public ActionResult Index()
        {
            return RedirectToAction("Search");
        }

        public ActionResult Search(string query, int? page, List<bool> genders, List<Guid> facultiesSelected)
        {
            ViewBag.Host = (Request.Url == null ? "" : Request.Url.Host);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here

            if (query == "")
            {
                query = null;
            }

            ViewBag.QueryData = query;
            var pageNumber = (page ?? 1);
            const int pageSize = 5;

            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var listData = workScope.Doctors.Query(x => !x.IsDelete).ToList();
                var faculties = workScope.Faculties.GetAll().ToList();
                ViewBag.Faculties = faculties;

                double elapsedMs = 0;

                var q = (
                    from mt in listData
                    join f in faculties on mt.FacultyId equals f.Id

                    select mt).AsQueryable();

                if (!string.IsNullOrEmpty(query))
                    q = q.Where(x => x.Name.ToLower().Contains(query.Trim().ToLower())
                                     || !string.IsNullOrEmpty(x.Descriptions) && x.Descriptions.ToLower().Contains(query.Trim().ToLower())
                                     || !string.IsNullOrEmpty(x.Faculty.Name) && x.Faculty.Name.ToLower().Contains(query.Trim().ToLower())

                    );

                if (facultiesSelected != null && facultiesSelected.Count > 0)
                    q = q.Where(x => facultiesSelected.Contains(x.Faculty.Id));
                if (genders != null && genders.Count > 0)
                    q = q.Where(x => genders != null && genders.Contains(x.Gender));

                ViewBag.Total = q.Count();
                ViewBag.FacultiesSelected = facultiesSelected;
                ViewBag.Genders = genders;
                watch.Stop();

                elapsedMs = (double)watch.ElapsedMilliseconds / 1000;
                ViewBag.RequestTime = elapsedMs;
                return View(q.ToPagedList(pageNumber, pageSize));
            }
        }

        public ActionResult Detail(Guid id)
        {
            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var doctor = workScope.Doctors.Include(x => x.Faculty).FirstOrDefault(x => x.Id == id);
                if (doctor != null)
                {
                    return View(doctor);
                }
                else
                {
                    return RedirectToAction("Search");
                }
            }
        }


        public ActionResult Booking(Guid id)
        {
            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var doctor = workScope.Doctors.Include(x => x.Faculty).FirstOrDefault(x => x.Id == id);
                if (doctor != null)
                {
                    return View(doctor);
                }
                else
                {
                    return RedirectToAction("Search");
                }
            }
        }
        [HttpPost]
        public JsonResult GetJson(Guid? id)
        {
            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var doctor = workScope.Doctors.FirstOrDefault(x => x.Id == id && !x.IsDelete);

                return doctor == default ?
                    Json(new
                    {
                        status = false,
                        mess = "Có lỗi xảy ra: "
                    }) :
                    Json(new
                    {
                        status = true,
                        mess = "Lấy thành công ",
                        data = new
                        {
                            doctor.Id,
                            doctor.Name,
                            doctor.Address,
                            doctor.Email,
                            doctor.Phone,
                            doctor.FacultyId,
                            doctor.Gender,
                            doctor.Avatar,
                            doctor.Descriptions
                        }
                    });
            }
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult Book(Guid doctorId, DateTime time, FrameTimeEnum frameTime)
        {
            var user = CookiesManage.GetUser();

            if (user == null)
            {
                return Json(new { status = false, mess = "Vui lòng đăng nhập" });
            }

            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                // check Khung giờ bác sỹ rảnh
                var isExist = workScope.DoctorSchedules.Query(x => x.DoctorId == doctorId && x.ScheduleBook.Year == time.Year && x.ScheduleBook.Month == time.Month && x.ScheduleBook.Day == time.Day && x.ScheduleBook.Hour == time.Hour && x.ScheduleBook.Minute == time.Minute && x.FrameTime == frameTime).Any();
                if (isExist)
                {
                    return Json(new { status = false, mess = "Khung giờ này không khả dụng, hệ thống tự động đề xuất.", isExist = true });
                }

                if (time < DateTime.Now)
                {
                    return Json(new { status = false, mess = "Không thể đặt lịch trước thời gian hiện tại." });
                }

                var doctor = workScope.Doctors.FirstOrDefault(x => x.Id == doctorId && !x.IsDelete);
                if (doctor == null)
                    return Json(new { status = true, mess = "Cập nhật thành công " });

                workScope.DoctorSchedules.Add(new DoctorSchedule
                {
                    DoctorId = doctor.Id,
                    PatientId = user.PatientId.GetValueOrDefault(),
                    ScheduleBook = time,
                    Status = BookingStatusKey.Pending,
                    FrameTime = frameTime,
                });
                workScope.Complete();

                var patientDoctor = workScope.PatientDoctors.FirstOrDefault(x =>
                    x.DoctorId == doctor.Id && x.PatientId == user.PatientId);

                if (patientDoctor != null)
                    return Json(new { status = true, mess = "Cập nhật thành công " });

                workScope.PatientDoctors.Add(new PatientDoctor
                {
                    PatientId = user.PatientId.GetValueOrDefault(),
                    DoctorId = doctor.Id,
                    Status = 1
                });
                workScope.Complete();

                return Json(new { status = true, mess = "Cập nhật thành công " });
            }
        }


        [HttpPost, ValidateInput(false)]
        public JsonResult UpdateBooking(Guid doctorId, DateTime time, FrameTimeEnum frameTime)
        {
            try
            {
                var user = CookiesManage.GetUser();

                if (user == null)
                {
                    return Json(new { status = false, mess = "Vui lòng đăng nhập" });
                }

                using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
                {
                    var doctorSchedule = workScope.DoctorSchedules.Query(x => x.DoctorId == doctorId && x.ScheduleBook.Year == time.Year && x.ScheduleBook.Month == time.Month && x.ScheduleBook.Day == time.Day && x.FrameTime == frameTime).FirstOrDefault();
                    doctorSchedule.FrameTime = frameTime;
                    workScope.DoctorSchedules.Put(doctorSchedule, doctorSchedule.Id);
                    workScope.Complete();
                    return Json(new { status = true, mess = "Cập nhập thành công " });
                }
            }
            catch (Exception)
            {
                return Json(new { status = false, mess = "Cập nhập lỗi" });
            }
        }


        [HttpPost]
        public JsonResult GetDoctorPropose(Guid doctorId, DateTime time, FrameTimeEnum frameTime)
        {
            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var doctor = workScope.Doctors.FirstOrDefault(x => x.Id == doctorId && !x.IsDelete);
                if (doctor != null)
                {
                    var doctorNew = workScope.Doctors.Include(x => x.DoctorSchedules).Where(x => x.FacultyId == doctor.FacultyId && !x.IsDelete && x.Id != doctorId && x.DoctorSchedules.Where(z => z.ScheduleBook.Year == time.Year && z.ScheduleBook.Month == time.Month && z.ScheduleBook.Day == time.Day && z.FrameTime == frameTime).Count() == 0).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                    if (doctorNew != null)
                    {
                        return Json(new
                        {
                            status = true,
                            mess = "Lấy thành công ",
                            isLoadInfo = true,
                            data = doctorNew.Id
                        }, JsonRequestBehavior.AllowGet);


                    }
                    else
                    {
                        return Json(new { status = true, mess = "Hiện tại các bác sĩ đều bận, vui lòng thử lại sau!", isLoadInfo = false }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { status = false, mess = "Có lỗi xảy ra:" }, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult GetListFrameTimes(Guid doctorId, DateTime time/*, FrameTimeEnum frameTime*/)
        {
            using (var workScope = new UnitOfWork(new HospitalManagementDbContext()))
            {
                var frameTimes = Enum.GetValues(typeof(FrameTimeEnum)).Cast<FrameTimeEnum>().ToList().Select(x => new System.Web.Mvc.SelectListItem
                {
                    Text = x.GetEnumDescription(),
                    Value = ((int)x).ToString(),
                });
                var results = new List<System.Web.Mvc.SelectListItem>();
                foreach (var item in frameTimes)
                {
                    var doctorSchedules = workScope.DoctorSchedules.Query(x => x.DoctorId == doctorId && x.ScheduleBook.Year == time.Year && x.ScheduleBook.Month == time.Month && x.ScheduleBook.Day == time.Day && ((int)x.FrameTime).ToString() == item.Value).ToList();
                    //if (doctorSchedules.Count > 0)
                    //{
                    //    var a = doctorSchedules.Where(x => ((int)x.FrameTime).ToString() == item.Value);
                    //    item.Disabled = a != null;
                    //}
                    item.Disabled = doctorSchedules.Count > 0;
                    results.Add(item);
                }

                return Json(new { data = results }, JsonRequestBehavior.AllowGet);           }
        }

    }
}