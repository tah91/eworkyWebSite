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
    
    public partial class MemberQuotation
    {
        // Primitive properties
    
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int OfferId { get; set; }
        public decimal Surface { get; set; }
        public string Message { get; set; }
        public decimal Price { get; set; }
        public bool Handled { get; set; }
        public bool Confirmed { get; set; }
        public bool Refused { get; set; }
    
        // Navigation properties
    
        public virtual Member Member { get; set; }
        public virtual Offer Offer { get; set; }
    
    }
}
