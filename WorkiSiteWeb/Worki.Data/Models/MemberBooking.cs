//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Worki.Data.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class MemberBooking
    {
        public MemberBooking()
        {
            this.MemberBookingLogs = new HashSet<MemberBookingLog>();
            this.Transactions = new HashSet<Transaction>();
    		OnInitialized();
        }
    
    	// Custom initialisation inside constructor
    	// Be aware this is called before loading data from store
    	partial void OnInitialized();
    
        // Primitive properties
    
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int LocalisationId { get; set; }
        public int OfferId { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public string Message { get; set; }
        public int Price { get; set; }
        public int StatusId { get; set; }
        public string PaymentMail { get; set; }
    
        // Navigation properties
    
        public virtual Member Member { get; set; }
        public virtual Offer Offer { get; set; }
        public virtual ICollection<MemberBookingLog> MemberBookingLogs { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    
    }
}
