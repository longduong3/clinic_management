using BELibrary.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using static BELibrary.Models.Constants;

namespace HospitalManagement.Areas.Admin.Models
{
    public class DoctorScheduleViewModel
    {
        public DoctorScheduleViewModel()
        {
        }
        public string StartDate { get; set; }
        public string PatientName { get; set; }
        public DateTime ScheduleBook { get; set; }
        public int Status { get; set; }
        public FrameTimeEnum? FrameTime { get; set; }
        public string StartDateTime { 
            get {
                if (FrameTime == null)
                {
                    return StartDate;
                }
                else
                {
                    // "7:30 - 8:30"
                    var frameTime = FrameTime.GetEnumDescription();
                    var arr = frameTime?.Split('-');
                    if (arr.Length >= 2)
                    {
                        //2023-01-12T12:00:00
                        return $"{StartDate}T{arr[0].Trim()}";
                    }
                    else
                    {
                        return StartDate;
                    }
                }
            } 
        }

        public string EndDateTime
        {
            get
            {
                if (FrameTime == null)
                {
                    return StartDate;
                }
                else
                {
                    // "7:30 - 8:30"
                    var frameTime = FrameTime.GetEnumDescription();
                    var arr = frameTime?.Split('-');
                    if (arr.Length >= 2)
                    {
                        //2023-01-12T12:00:00
                        return $"{StartDate}T{arr[1].Trim()}";
                    }
                    else
                    {
                        return StartDate;
                    }
                }
            }
        }
    }
}