//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Pawn.Core.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tb_ExtentionContract
    {
        public int Id { get; set; }
        public int ContractID { get; set; }
        public int AddTime { get; set; }
        public string Note { get; set; }
        public string CreatedUser { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string DeletedUser { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
        public int DocumentType { get; set; }
        public Nullable<System.DateTime> ToDateContract { get; set; }
    }
}
