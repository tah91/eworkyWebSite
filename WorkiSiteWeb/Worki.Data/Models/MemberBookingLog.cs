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
    
    public partial class MemberBookingLog
    {
        // Primitive properties
    
        public int Id { get; set; }
        public int MemberBookingId { get; set; }
        public int MemberId { get; set; }
        public int LocalisationId { get; set; }
        public int OfferId { get; set; }
        public string Event { get; set; }
    
        // Navigation properties
    
        public virtual MemberBooking MemberBooking { get; set; }
    
    }
}
