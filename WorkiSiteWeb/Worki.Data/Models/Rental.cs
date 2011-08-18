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
    
    public partial class Rental
    {
        public Rental()
        {
            this.RentalAccesses = new HashSet<RentalAccess>();
            this.RentalFeatures = new HashSet<RentalFeature>();
            this.RentalFiles = new HashSet<RentalFile>();
        }
    
        // Primitive properties
    
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string Reference { get; set; }
        public int Type { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public Nullable<System.DateTime> AvailableDate { get; set; }
        public bool AvailableNow { get; set; }
        public int LeaseType { get; set; }
        public int Rate { get; set; }
        public int Charges { get; set; }
        public string Description { get; set; }
        public int Energy { get; set; }
        public int GreenHouse { get; set; }
        public int HeatingType { get; set; }
        public System.DateTime TimeStamp { get; set; }
    
        // Navigation properties
    
        public virtual Member Member { get; set; }
        public virtual ICollection<RentalAccess> RentalAccesses { get; set; }
        public virtual ICollection<RentalFeature> RentalFeatures { get; set; }
        public virtual ICollection<RentalFile> RentalFiles { get; set; }
    
    }
}
