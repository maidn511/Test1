using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pawn.Models
{
    public class MenuPermissionModel
    {
        public MenuPermissionModel()
        {
            Roles = new List<BaseItemModel>();
            Menus = new List<BaseItemModel>();
            Allowed = new Dictionary<int, Dictionary<int, bool>>();
        }
        public List<BaseItemModel> Roles { get; set; }
        public List<BaseItemModel> Menus { get; set; }
        /// <summary>
        /// RoleId, MenuId, 1 role has many menu
        /// </summary>
        public Dictionary<int, Dictionary<int, bool>> Allowed { get; set; }
    }
}