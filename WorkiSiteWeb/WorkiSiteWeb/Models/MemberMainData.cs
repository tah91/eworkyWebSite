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
    
    public partial class MemberMainData
    {
        // Primitive properties
    
        public int RelationId { get; set; }
        public int MemberId { get; set; }
        public int Civility { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Avatar { get; set; }
        public string CompanyName { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string WorkCity { get; set; }
        public string WorkPostalCode { get; set; }
        public string WorkCountry { get; set; }
        public int Profile { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; }
        public string FavoritePlaces { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Linkedin { get; set; }
        public string Viadeo { get; set; }
    
        // Navigation properties
    
        public virtual Member Member { get; set; }
    
    }
}
