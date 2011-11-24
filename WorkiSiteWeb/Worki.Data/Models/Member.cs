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
    
    public partial class Member
    {
        public Member()
        {
            this.Comments = new HashSet<Comment>();
            this.FavoriteLocalisations = new HashSet<FavoriteLocalisation>();
            this.MemberEditions = new HashSet<MemberEdition>();
            this.MembersInGroups = new HashSet<MembersInGroup>();
            this.Rentals = new HashSet<Rental>();
            this.WelcomePeoples = new HashSet<WelcomePeople>();
            this.Localisations = new HashSet<Localisation>();
            this.MemberBookings = new HashSet<MemberBooking>();
            this.MemberQuotations = new HashSet<MemberQuotation>();
            this.Transactions = new HashSet<Transaction>();
    		OnInitialized();
        }
    
    	// Custom initialisation inside constructor
    	// Be aware this is called before loading data from store
    	partial void OnInitialized();
    
        // Primitive properties
    
        public int MemberId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }
        public string EmailKey { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime LastActivityDate { get; set; }
        public System.DateTime LastLoginDate { get; set; }
        public System.DateTime LastPasswordChangedDate { get; set; }
        public System.DateTime LastLockoutDate { get; set; }
        public int FailedPasswordAttemptCount { get; set; }
        public System.DateTime FailedPasswordAttemptWindowStart { get; set; }
        public int FailedPasswordAnswerAttemptCount { get; set; }
        public System.DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }
        public string Comment { get; set; }
    
        // Navigation properties
    
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<FavoriteLocalisation> FavoriteLocalisations { get; set; }
        public virtual ICollection<MemberEdition> MemberEditions { get; set; }
        public virtual MemberMainData MemberMainData { get; set; }
        public virtual ICollection<MembersInGroup> MembersInGroups { get; set; }
        public virtual ICollection<Rental> Rentals { get; set; }
        public virtual ICollection<WelcomePeople> WelcomePeoples { get; set; }
        public virtual ICollection<Localisation> Localisations { get; set; }
        public virtual ICollection<MemberBooking> MemberBookings { get; set; }
        public virtual ICollection<MemberQuotation> MemberQuotations { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    
    }
}
