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
    
    public partial class Tb_Capital
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string DocumentName { get; set; }
        public Nullable<int> DocumentType { get; set; }
        public System.DateTime DocumentDate { get; set; }
        public string Phone { get; set; }
        public decimal MoneyNumber { get; set; }
        public int Method { get; set; }
        public bool IsBeforeReceipt { get; set; }
        public Nullable<decimal> InterestRate { get; set; }
        public Nullable<int> RateType { get; set; }
        public Nullable<int> BorrowNumber { get; set; }
        public Nullable<int> BorrowPeriod { get; set; }
        public string Note { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
        public string DeletedUser { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public Nullable<int> StatusContractId { get; set; }
        public Nullable<int> Status { get; set; }
        public bool IsSystem { get; set; }
        public bool IsClose { get; set; }
        public Nullable<System.DateTime> CloseDate { get; set; }
        public string CloseUser { get; set; }
    }
}
