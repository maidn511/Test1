using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pawn.Models
{
    public class MenuModels : ViewModel.Models.MenuModels
    {
        public MenuModels() { }
        public List<MenuModels> LstMenuChild { get; set; }
    }
}