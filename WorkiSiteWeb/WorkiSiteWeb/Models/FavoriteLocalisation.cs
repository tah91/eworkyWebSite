//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WorkiSiteWeb.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class FavoriteLocalisation
    {
        // Primitive properties
    
        public int RelationId { get; set; }
        public int MemberId { get; set; }
        public int LocalisationId { get; set; }
    
        // Navigation properties
    
        public virtual Localisation Localisation { get; set; }
        public virtual Member Member { get; set; }
    
    }
}
