using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BELibrary.Models
{
    public static class Constants
    {
        public enum FrameTimeEnum
        {
            [Description("07:30 - 07:45")]
            FrameTime1 = 1,
            [Description("07:45 - 08:00")]
            FrameTime2 = 2,
            [Description("08:00 - 08:15")]
            FrameTime3 = 3,
            [Description("08:15 - 08:30")]
            FrameTime4 = 4,
            [Description("08:30 - 08:45")]
            FrameTime5 = 5,
            [Description("08:45 - 09:00")]
            FrameTime6 = 6,
            [Description("09:00 - 09:15")]
            FrameTime7 = 7,
            [Description("09:15 - 09:30")]
            FrameTime8 = 8,
            [Description("09:30 - 09:45")]
            FrameTime9 = 9,
            [Description("09:45 - 10:00")]
            FrameTime10 = 10,
            [Description("10:00 - 10:15")]
            FrameTime11 = 11,
            [Description("10:15 - 10:30")]
            FrameTime12 = 12,
            [Description("10:30 - 10:45")]
            FrameTime13 = 13,
            [Description("10:45 - 11:00")]
            FrameTime14 = 14,
            [Description("11:00 - 11:15")]
            FrameTime15 = 15,
            [Description("11:15 - 11:30")]
            FrameTime16 = 16,
            [Description("11:30 - 11:45")]
            FrameTime17 = 17,
            [Description("11:45 - 12:00")]
            FrameTime18 = 18,
            [Description("12:00 - 12:15")]
            FrameTime19 = 19,
            [Description("12:15 - 12:30")]
            FrameTime20 = 20,
            [Description("12:30 - 12:45")]
            FrameTime21 = 21,
            [Description("12:45 - 13:00")]
            FrameTime22 = 22,
            [Description("13:00 - 13:15")]
            FrameTime23 = 23,
            [Description("13:15 - 13:30")]
            FrameTime24 = 24,
            [Description("13:30 - 13:45")]
            FrameTime25 = 25,
            [Description("13:45 - 14:00")]
            FrameTime26 = 26,
            [Description("14:00 - 14:15")]
            FrameTime27 = 27,
            [Description("14:15 - 14:30")]
            FrameTime28 = 28,
            [Description("14:30 - 14:45")]
            FrameTime29 = 29,
            [Description("14:45 - 15:00")]
            FrameTime30 = 30,
            [Description("15:00 - 15:15")]
            FrameTime31 = 31,
            [Description("15:15 - 15:30")]
            FrameTime32 = 32,
            [Description("15:30 - 15:45")]
            FrameTime33 = 33,
            [Description("15:45 - 16:00")]
            FrameTime34 = 34,
            [Description("16:00 - 16:15")]
            FrameTime35 = 35,
            [Description("16:15 - 16:30")]
            FrameTime36 = 36,
            [Description("16:30 - 16:45")]
            FrameTime37 = 37,
            [Description("16:45 - 17:00")]
            FrameTime38 = 38,
        }
        public static string GetEnumDescription(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
        }

        public static bool ValidTime(this FrameTimeEnum frame, DateTime date)
        {
            if (date.Date > DateTime.Now.Date)
                return true;
            else if (date.Date < DateTime.Now.Date)
                return false;

            var hour = DateTime.Now.Hour;
            var minute = DateTime.Now.Minute;
            switch (frame)
            {
                case FrameTimeEnum.FrameTime1:
                    return hour < 7 || (hour == 7 && minute < 15);
                case FrameTimeEnum.FrameTime3:
                    return hour < 8 || (hour == 8 && minute < 15);
                case FrameTimeEnum.FrameTime6:
                    return hour < 9 || (hour == 9 && minute < 15);
                case FrameTimeEnum.FrameTime9:
                    return hour < 10 || (hour == 10 && minute < 15);
                case FrameTimeEnum.FrameTime12:
                    return hour < 13 || (hour == 13 && minute < 15);
                case FrameTimeEnum.FrameTime15:
                    return hour < 14 || (hour == 14 && minute < 15);
                case FrameTimeEnum.FrameTime18:
                    return hour < 15 || (hour == 15 && minute < 15);
                case FrameTimeEnum.FrameTime21:
                    return hour < 16 || (hour == 16 && minute < 15);
                case FrameTimeEnum.FrameTime38:
                    return hour < 17 || (hour == 17 && minute < 15);
                default:
                    return false;
            }
        }
    }
}
