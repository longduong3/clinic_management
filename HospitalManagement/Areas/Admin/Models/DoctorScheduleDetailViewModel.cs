using BELibrary.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace HospitalManagement.Areas.Admin.Models
{
    public class DoctorScheduleDetailViewModel
    {
        public string StartDate { get; set; }
        public List<DoctorScheduleViewModel> Details { get; set; }
    }
}