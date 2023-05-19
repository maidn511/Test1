using Newtonsoft.Json;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Pawn.Libraries
{
    public static class Utility
    {
        /// <summary>
        /// Convert Json to Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pSerializeJSON"></param>
        /// <returns>T</returns>
        public static T ToObject<T>(this string pSerializeJSON) where T : new ()
        {
            if (pSerializeJSON == null || string.IsNullOrEmpty(pSerializeJSON.Trim()))
                return new T();
            var obj = JsonConvert.DeserializeObject<T>(pSerializeJSON);
            return obj;
        }

        /// <summary>
        /// Convert Json to List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pSerializeJSON"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this string pSerializeJSON)
        {
            if (pSerializeJSON == null || string.IsNullOrEmpty(pSerializeJSON.Trim()))
                return new List<T>();
            var list = JsonConvert.DeserializeObject<List<T>>(pSerializeJSON);
            return list;
        }

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// Hàm định dạng. Ex: 10000 => 10.000
        /// </summary>
        /// <param name="number">Tham số định dạng</param>
        /// <param name="strPrefix">Dấu ngăn cách</param>
        /// <returns></returns>
        public static string FormatPrice(this string strNumber, string strPrefix)
        {
            var str1 = strNumber;
            var str2 = "";
            for (; str1.Length > 3; str1 = str1.Substring(0, str1.Length - 3))
                str2 = strPrefix + str1.Substring(str1.Length - 3) + str2;
            return str1 + str2;
        }

        public static string ToPrice(this object price)
        {
            var rs = string.Format("{0:0,0}", price);
            rs = rs == "00" ? "0" : rs;
            return rs;
        }

        public static string FormatPrice(this long price, string strPrefix)
        {
            string str1 = price.ToString();
            string str2 = "";
            for (; str1.Length > 3; str1 = str1.Substring(0, str1.Length - 3))
                str2 = strPrefix + str1.Substring(str1.Length - 3) + str2;
            return str1 + str2;
        }

        public static string FormatPrice(this int price, string strPrefix)
        {
            string str1 = price.ToString();
            string str2 = "";
            for (; str1.Length > 3; str1 = str1.Substring(0, str1.Length - 3))
                str2 = strPrefix + str1.Substring(str1.Length - 3) + str2;
            return str1 + str2;
        }

        /// <summary>
        /// Hàm convert sang kiểu int null hoặc lỗi trả về -1
        /// </summary>
        /// <param name="number">Tham số truyền vào</param>
        /// <returns></returns>
        public static int ToInt32(this string number)
        {
            int result;
            return int.TryParse(number, out result) ? Convert.ToInt32(number) : -1;
        }
        
        /// <summary>
        /// Hàm convert sang kiểu int null hoặc lỗi trả về -1
        /// </summary>
        /// <param name="number">Tham số truyền vào</param>
        /// <returns></returns>
        public static int ToInt32Zero(this string number)
        {
            int result;
            return int.TryParse(number, out result) ? Convert.ToInt32(number) : -1;
        }

        /// <summary>
        /// Hàm convert sang kiểu long null hoặc lỗi trả về -1
        /// </summary>
        /// <param name="number">Tham số truyền vào</param>
        /// <returns></returns>
        public static long ToLong(this string number)
        {
            long result;
            return long.TryParse(number, out result) ? result : -1;
        }

        /// <summary>
        /// Convert string to datetime
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string datetime, string format)
        {
            try
            {
                return DateTime.ParseExact(datetime, format, (IFormatProvider)CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Convert string to datetime
        /// Exception return null
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static DateTime? ToDateTimeNull(this string datetime, string format)
        {
            try
            {
                return DateTime.ParseExact(datetime, format, (IFormatProvider)CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetCurrentIp()
        {
            string str = string.Empty;
            if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                str = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            else if (!string.IsNullOrEmpty(HttpContext.Current.Request.UserHostAddress))
                str = HttpContext.Current.Request.UserHostAddress;
            return str;
        }

        public static string ToInterval(this DateTime dt)
        {
            TimeSpan timeSpan = DateTime.Now - dt;
            if (timeSpan.Days == 0 && timeSpan.Hours > 0)
                return "khoảng " + (object)timeSpan.Hours + " tiếng trước";
            if (timeSpan.Days == 0 && timeSpan.Minutes > 0 && timeSpan.Hours == 0)
                return "khoảng " + (object)timeSpan.Minutes + " phút trước";
            if (timeSpan.Days == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds > 0)
                return "cách đây khoảng " + (object)timeSpan.Seconds + " giây";
            if (timeSpan.Days == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 0)
                return "vừa xong";
            if (timeSpan.Days > 0 && timeSpan.Days <= 7)
                return "cách đây " + (object)timeSpan.Days + " ngày";
            return dt.ToString("HH:mm") + " " + (object)dt.Day + "/" + (object)dt.Month + "/" + (object)dt.Year;
        }
        /// <summary>
        /// 2 class same about property
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDestination Map<TDestination>(this object source)
        {
            var r = Activator.CreateInstance<TDestination>();
            r.PopulateWith(source);
            return r;
        }


        public static int CodeBHContractToInt(this string code)
        {
            int temp = 0;
            if(int.TryParse(code.Replace("BH-", "").Trim(), out temp ))
            {
                return int.Parse(code.Replace("BH-", "").Trim());
            }
            return 0;
        }

        public static string DescriptionAttr<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }

        public static string GetDescriptionC6(Enum value)
        {
            return
                value
                    .GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description;
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static string GetDescription(System.Enum value)
        {
            var enumMember = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var descriptionAttribute =
                enumMember == null
                    ? default(DescriptionAttribute)
                    : enumMember.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
            return
                descriptionAttribute == null
                    ? value.ToString()
                    : descriptionAttribute.Description;
        }

        public static int DocumentTypeAttr<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DocumentTypeAttribute[] attributes = (DocumentTypeAttribute[])fi.GetCustomAttributes(
                typeof(DocumentTypeAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].DocType;
            else return Convert.ToInt32(source.ToString());
        }
        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            return type.GetField(name) // I prefer to get attributes this way
                .GetCustomAttribute<TAttribute>();
        }

        public class DocumentTypeAttribute : Attribute
        {
            internal DocumentTypeAttribute(int docType)
            {
                this.DocType = docType;
            }
            public int DocType { get; private set; }
        }
    }
}
