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
    
    public partial class OfferPrice
    {
        // Primitive properties
    
        public int Id { get; set; }
        public int OfferId { get; set; }
        public decimal Price { get; set; }
        public int PriceType { get; set; }
    
        // Navigation properties
    
        public virtual Offer Offer { get; set; }
    
    }
}
