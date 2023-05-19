using Pawn.Libraries;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Pawn.Authorize
{
    public class RDAuthorize
    {
        public static void Set(AccountModels accountModels)
        {

            HttpContext.Current.Session[Constants.userInfo] = accountModels;
            HttpContext.Current.Session[Constants.userid] = accountModels.Id;
            HttpContext.Current.Session[Constants.username] = accountModels.Username;
            HttpContext.Current.Session[Constants.firstname] = accountModels.Firstname;
            HttpContext.Current.Session[Constants.lastname] = accountModels.Lastname;
            HttpContext.Current.Session[Constants.address] = accountModels.Address;
            HttpContext.Current.Session[Constants.fullname] = accountModels.Firstname + " " + accountModels.Lastname;
            HttpContext.Current.Session[Constants.avatar] = string.IsNullOrEmpty(accountModels.Avatar) ? "/Content/images/gallery/4.jpg" : accountModels.Avatar;
            HttpContext.Current.Session[Constants.permission] = accountModels.ListRoles;
            HttpContext.Current.Session[Constants.store] = accountModels.Store;
            HttpContext.Current.Session[Constants.storeList] = accountModels.ListStores;
            HttpContext.Current.Session[Constants.role] = accountModels.ListRole;
        }

        public static bool IsPermission(string key)
        {
            return true;
            if (HttpContext.Current.Session[Constants.accounttype] + "" == ConfigurationManager.AppSettings["GroupAdmin"] + "")
            {
                return true;
            }
            bool isHasPermission = false;
            if (!string.IsNullOrEmpty(key))
            {
                var listPermission = (List<RoleModels>)HttpContext.Current.Session[Constants.permission];
                if (listPermission != null)
                {
                    isHasPermission = listPermission.FirstOrDefault(c => c.RoleName.ToLower() == key.ToLower()) != null;
                }
            }
            return isHasPermission;
        }
        public static bool IsPermissionConfig(string key)
        {
            return true;
            if (HttpContext.Current.Session[Constants.accounttype] + "" == ConfigurationManager.AppSettings["GroupAdmin"])
            {
                return true;
            }
            bool isHasPermission = false;
            if (!string.IsNullOrEmpty(key))
            {
                var pesmissionKey = ConfigurationManager.AppSettings[key];
                var listPermission = (List<RoleModels>)HttpContext.Current.Session[Constants.permission];
                if (listPermission != null)
                {
                    isHasPermission = listPermission.FirstOrDefault(c => c.RoleName.ToLower() == pesmissionKey.ToLower()) != null;
                }
            }
            return isHasPermission;
        }

        public string SignInUrl { get; set; }

        public static bool IsLogin => HttpContext.Current.Session[Constants.userInfo] != null;

        public static int UserId => Convert.ToInt32(HttpContext.Current.Session[Constants.userid]);

        public static string Username => Convert.ToString(HttpContext.Current.Session[Constants.username]);

        public static string Firstname => Convert.ToString(HttpContext.Current.Session[Constants.firstname]);

        public static string Lastname => Convert.ToString(HttpContext.Current.Session[Constants.lastname]);
    
        public static string Fullname => Convert.ToString(HttpContext.Current.Session[Constants.fullname]);

        public static string Address => Convert.ToString(HttpContext.Current.Session[Constants.address]);

        public static string Avatar => Convert.ToString(HttpContext.Current.Session[Constants.avatar]);
        public static int AccountType => Convert.ToInt32(HttpContext.Current.Session[Constants.accounttype]);

        public static List<PawnStoreModels> StoreList => (List<PawnStoreModels>)HttpContext.Current.Session[Constants.storeList] ?? new List<PawnStoreModels>();
        public static List<int> ListRole => (List<int>)HttpContext.Current.Session[Constants.role] ?? new List<int>();

        public static PawnStoreModels Store => (PawnStoreModels)HttpContext.Current.Session[Constants.store] ?? new PawnStoreModels();

        public string Permission;

        public static bool IsRoot => ListRole.Contains(RoleEnum.Root);
        public static bool IsAdmin => ListRole.Contains(RoleEnum.Admin);
        public static bool IsUserStore => ListRole.Contains(RoleEnum.Store);
        public static bool IsUser => ListRole.Contains(RoleEnum.UserVN);
    }
}
