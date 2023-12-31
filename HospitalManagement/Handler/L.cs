﻿using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace HospitalManagement.Handler
{
    public class PayCompare
    {
        public static string T(string param)
        {
            string language = CookiesManage.GetLanguageCode();
            var info = new CultureInfo(language);
            Thread.CurrentThread.CurrentCulture = info;
            Thread.CurrentThread.CurrentUICulture = info;
            var rs = new ResourceManager("Resources.Resource", Assembly.Load("App_GlobalResources"));
            return rs.GetString(param);
        }
    }
}