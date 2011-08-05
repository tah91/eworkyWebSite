//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Worki.Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class MemberBooking
    {
        // Primitive properties
    
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int LocalisationId { get; set; }
        public int Offer { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public string Message { get; set; }
        public bool Handled { get; set; }
        public bool Confirmed { get; set; }
        public int Price { get; set; }
    
        // Navigation properties
    
        public virtual Localisation Localisation { get; set; }
        public virtual Member Member { get; set; }
    
    }
}
