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
    
    public partial class Tb_IncomeAndExpense
    {
        public int Id { get; set; }
        public string DocumentName { get; set; }
        public Nullable<int> DocumentType { get; set; }
        public Nullable<System.DateTime> DocumentDate { get; set; }
        public string VoucherCode { get; set; }
        public Nullable<int> VoucherType { get; set; }
        public string Customer { get; set; }
        public Nullable<decimal> MoneyNumber { get; set; }
        public Nullable<int> Method { get; set; }
        public string Reason { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
        public string DeletedUser { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<int> StoreId { get; set; }
    }
}