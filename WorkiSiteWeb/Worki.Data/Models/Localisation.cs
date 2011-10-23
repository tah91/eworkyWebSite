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
            this.MemberBookings = new HashSet<MemberBooking>();
            this.MemberEditions = new HashSet<MemberEdition>();
            this.WelcomePeoples = new HashSet<WelcomePeople>();
            this.Offers = new HashSet<Offer>();
    		OnInitialized();
        }
    
    	// Custom initialisation inside constructor
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
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Nullable<int> OwnerID { get; set; }
        public bool Bookable { get; set; }
        public Nullable<System.DateTime> MonOpen { get; set; }
        public Nullable<System.DateTime> MonClose { get; set; }
        public Nullable<System.DateTime> MonOpen2 { get; set; }
        public Nullable<System.DateTime> MonClose2 { get; set; }
        public Nullable<System.DateTime> TueOpen { get; set; }
        public Nullable<System.DateTime> TueClose { get; set; }
        public Nullable<System.DateTime> TueOpen2 { get; set; }
        public Nullable<System.DateTime> TueClose2 { get; set; }
        public Nullable<System.DateTime> WedOpen { get; set; }
        public Nullable<System.DateTime> WedClose { get; set; }
        public Nullable<System.DateTime> WedOpen2 { get; set; }
        public Nullable<System.DateTime> WedClose2 { get; set; }
        public Nullable<System.DateTime> ThuOpen { get; set; }
        public Nullable<System.DateTime> ThuClose { get; set; }
        public Nullable<System.DateTime> ThuOpen2 { get; set; }
        public Nullable<System.DateTime> ThuClose2 { get; set; }
        public Nullable<System.DateTime> FriOpen { get; set; }
        public Nullable<System.DateTime> FriClose { get; set; }
        public Nullable<System.DateTime> FriOpen2 { get; set; }
        public Nullable<System.DateTime> FriClose2 { get; set; }
        public Nullable<System.DateTime> SatOpen { get; set; }
        public Nullable<System.DateTime> SatClose { get; set; }
        public Nullable<System.DateTime> SatOpen2 { get; set; }
        public Nullable<System.DateTime> SatClose2 { get; set; }
        public Nullable<System.DateTime> SunOpen { get; set; }
        public Nullable<System.DateTime> SunClose { get; set; }
        public Nullable<System.DateTime> SunOpen2 { get; set; }
        public Nullable<System.DateTime> SunClose2 { get; set; }
        public string PublicTransportation { get; set; }
        public string Station { get; set; }
        public string RoadAccess { get; set; }
    
        // Navigation properties
    
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<FavoriteLocalisation> FavoriteLocalisations { get; set; }
        public virtual Member Member { get; set; }
        public virtual LocalisationData LocalisationData { get; set; }
        public virtual ICollection<LocalisationFeature> LocalisationFeatures { get; set; }
        public virtual ICollection<LocalisationFile> LocalisationFiles { get; set; }
        public virtual MainLocalisation MainLocalisation { get; set; }
        public virtual ICollection<MemberBooking> MemberBookings { get; set; }
        public virtual ICollection<MemberEdition> MemberEditions { get; set; }
        public virtual ICollection<WelcomePeople> WelcomePeoples { get; set; }
        public virtual ICollection<Offer> Offers { get; set; }
    
    }
}
