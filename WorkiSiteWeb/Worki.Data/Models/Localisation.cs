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
    
    public partial class Localisation
    {
        public Localisation()
        {
            this.Comments = new HashSet<Comment>();
            this.FavoriteLocalisations = new HashSet<FavoriteLocalisation>();
            this.LocalisationFeatures = new HashSet<LocalisationFeature>();
            this.LocalisationFiles = new HashSet<LocalisationFile>();
            this.MemberEditions = new HashSet<MemberEdition>();
            this.Offers = new HashSet<Offer>();
            this.WelcomePeoples = new HashSet<WelcomePeople>();
    		OnInitialized();
        }
    
    	// Custom initialisation inside constructor
    	// Be aware this is called before loading data from store
    	partial void OnInitialized();
    
        // Primitive properties
    
        public int ID { get; set; }
        public string Name { get; set; }
        public int TypeValue { get; set; }
        public string Adress { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string Mail { get; set; }
        public string Fax { get; set; }
        public string WebSite { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Nullable<int> OwnerID { get; set; }
        public string PublicTransportation { get; set; }
        public string Station { get; set; }
        public string RoadAccess { get; set; }
        public decimal BookingCom { get; set; }
        public decimal QuotationPrice { get; set; }
    
        // Navigation properties
    
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<FavoriteLocalisation> FavoriteLocalisations { get; set; }
        public virtual LocalisationData LocalisationData { get; set; }
        public virtual ICollection<LocalisationFeature> LocalisationFeatures { get; set; }
        public virtual ICollection<LocalisationFile> LocalisationFiles { get; set; }
        public virtual MainLocalisation MainLocalisation { get; set; }
        public virtual ICollection<MemberEdition> MemberEditions { get; set; }
        public virtual ICollection<Offer> Offers { get; set; }
        public virtual ICollection<WelcomePeople> WelcomePeoples { get; set; }
        public virtual Member Member { get; set; }
    
    }
}
