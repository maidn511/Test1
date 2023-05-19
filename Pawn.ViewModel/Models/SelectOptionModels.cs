using System.Collections.Generic;

namespace Pawn.ViewModel.Models
{
    public class SelectOptionModels
    {
        public SelectOptionModels()
        {
            DataRequired = false;
            Multiple = false;
            Class = "form-control";
            IdSelect = "slDefault";
            IsSelectPicker = true;
            ListItems = new List<SelectListItemModels>();
        }

        public string IdSelect { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public bool DataRequired { get; set; }
        public bool Multiple { get; set; }
        public string DataKey { get; set; }
        public string Class { get; set; }
        public string ChosenSuggest { get; set; }
        public string Attribute { get; set; }
        public bool IsSelectPicker { get; set; }

        public string PlaceHolder { get; set; }
        public List<SelectListItemModels> ListItems { get; set; }
    }

    public class SelectListItemModels
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string Id { get; set; }
    }

    public class SelectOptionModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public int Id { get; set; }
        public int OrderIndex { get; set; }
    }
}
