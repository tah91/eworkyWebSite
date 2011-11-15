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
    using System.Data.Entity;
    
    public partial class WorkiDBEntities : DbContext
    {
        public WorkiDBEntities()
            : base("name=WorkiDBEntities")
        {
        }
    
        public DbSet<Comment> Comments { get; set; }
        public DbSet<FavoriteLocalisation> FavoriteLocalisations { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<LocalisationData> LocalisationDatas { get; set; }
        public DbSet<LocalisationFeature> LocalisationFeatures { get; set; }
        public DbSet<LocalisationFile> LocalisationFiles { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<MemberEdition> MemberEditions { get; set; }
        public DbSet<MemberMainData> MemberMainDatas { get; set; }
        public DbSet<MembersInGroup> MembersInGroups { get; set; }
        public DbSet<Press> Presses { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<RentalAccess> RentalAccesses { get; set; }
        public DbSet<RentalFeature> RentalFeatures { get; set; }
        public DbSet<RentalFile> RentalFiles { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<WelcomePeople> WelcomePeoples { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<OfferFile> OfferFiles { get; set; }
        public DbSet<OfferFeature> OfferFeatures { get; set; }
        public DbSet<MemberBooking> MemberBookings { get; set; }
        public DbSet<C__RefactorLog> C__RefactorLog { get; set; }
        public DbSet<Localisation> Localisations { get; set; }
        public DbSet<MainLocalisation> MainLocalisations { get; set; }
    }
}
