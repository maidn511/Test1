using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Libraries
{
    public class Constants
    {
        public static bool IsUsingCdn = Convert.ToBoolean(ConfigurationManager.AppSettings["IsUsingCdn"]);
        public static string UrlCdn = ConfigurationManager.AppSettings["UrlCdn"];
        public static string Host = ConfigurationManager.AppSettings["Host"];
        public static string UrlAccount = "Image_System/Account/" + DateTime.Now.ToString("yyyy/MM/dd");
        public static string UrlThumnail = "Image_System/Thumnail/" + DateTime.Now.ToString("yyyy/MM/dd");
        public static string UrlChungtu = "Image_System/ChungTu/" + DateTime.Now.ToString("yyyy/MM/dd");
        public static string UrlImage = "Image_System/" + DateTime.Now.ToString("yyyy/MM/dd");
        public static string Banner = "Image_System/Banner/" + DateTime.Now.ToString("yyyy/MM/dd");
        public static string DateFormat = ConfigurationManager.AppSettings["DateFormat"] + "";
        public const string userInfo = "_userinfo";
        public const string userid = "_userid";
        public const string username = "_username";
        public const string firstname = "_firstname";
        public const string lastname = "_lastname";
        public const string address = "_address";
        public const string fullname = "_fullname";
        public const string role = "_role";
        public const string avatar = "_avatar";
        public const string permission = "_permission";
        public const string accounttype = "_accounttype";
        public const string store = "_store";
        public const string storeList = "_storeList";
        public const string isAdmin = "_isAdmin";
        public const string returnUrl = "_returUrl";
        public const string isRoot = "_isRoot";
    }
}
