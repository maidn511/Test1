using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pawn.Models
{
    public class BaseItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public int ParentID { get; set; }

        public int Level { get; set; }
    }

    public class FilterItemModel
    {
        public int Code { get; set; }
        public string Description { get; set; }
    }
}