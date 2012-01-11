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
    
    public partial class Offer
    {
        public Offer()
        {
            this.MemberBookings = new HashSet<MemberBooking>();
            this.OfferFeatures = new HashSet<OfferFeature>();
            this.OfferFiles = new HashSet<OfferFile>();
            this.MemberQuotations = new HashSet<MemberQuotation>();
            this.OfferPrices = new HashSet<OfferPrice>();
    		OnInitialized();
        }
    
    	// Custom initialisation inside constructor
    	// Be aware this is called before loading data from store
    	partial void OnInitialized();
    
        // Primitive properties
    
        public int Id { get; set; }
        public int LocalisationId { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public decimal Price { get; set; }
        public int Period { get; set; }
        public bool IsOnline { get; set; }
        public bool IsBookable { get; set; }
        public int PaymentType { get; set; }
        public int Currency { get; set; }
        public bool IsQuotable { get; set; }
        public Nullable<System.DateTime> AvailabilityDate { get; set; }
        public int AvailabilityPeriod { get; set; }
        public int AvailabilityPeriodType { get; set; }
    
        // Navigation properties
    
        public virtual ICollection<MemberBooking> MemberBookings { get; set; }
        public virtual ICollection<OfferFeature> OfferFeatures { get; set; }
        public virtual ICollection<OfferFile> OfferFiles { get; set; }
        public virtual ICollection<MemberQuotation> MemberQuotations { get; set; }
        public virtual Localisation Localisation { get; set; }
        public virtual ICollection<OfferPrice> OfferPrices { get; set; }
    
    }
}
