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
    
    public partial class MemberEdition
    {
        // Primitive properties
    
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int LocalisationId { get; set; }
        public System.DateTime ModificationDate { get; set; }
        public int ModificationType { get; set; }
    
        // Navigation properties
    
        public virtual Member Member { get; set; }
        public virtual Localisation Localisation { get; set; }
    
    }
}
