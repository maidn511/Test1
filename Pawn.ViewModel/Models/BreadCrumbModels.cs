using System.Collections.Generic;

namespace Pawn.ViewModel.Models
{
    public class BreadCrumbModels
    {
        public string Title { get; set; }
        public List<LiModels> ListLiModels { get; set; }
    }

    public class LiModels
    {
        public string Href { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public bool IsActive { get; set; }

        public LiModels()
        {
            Class = "breadcrumb-item";
        }
    }
}
