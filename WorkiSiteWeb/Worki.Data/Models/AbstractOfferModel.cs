using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.Linq;
using System.ComponentModel;

namespace Worki.Data.Models
{
    public class MasterOfferFormViewModel<T>
    {
        public T MemberOffer { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "PhoneNumber", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "FirstName", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "LastName", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public string LastName { get; set; }

        [Display(Name = "LocalisationName", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public string LocalisationName { get; set; }

        [Display(Name = "OfferName", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public string OfferName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Email", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public string Email { get; set; }

        public bool NeedNewAccount { get; set; }
    }

    public abstract class MasterMemberOffer
    {
        public enum Status
        {
            Unknown,
            Accepted,
            Refused,
            Cancelled,
            Paid
        }

        public abstract bool Unknown { get; }

        public abstract bool Refused { get; }

        public abstract bool Cancelled { get; }

        public abstract DateTime CreationDate { get; }

        public abstract DateTime RefusalDate { get; }

        public abstract DateTime CancellationDate { get; }

        public abstract bool ClientCanCancel { get; }

        public abstract void GetStatus(out string status, out string color, out DateTime? date);
    }

    public abstract class MasterMemberLog
    {
        public enum OfferEvent
        {
            General,
            Creation,
            Approval,
            Refusal,
            Payment,
            Cancellation
        }

        public abstract string GetDisplay();
    }

    public class MasterViewModel<T, U>
    {
        public PagingList<T> List { get; set; }
        public U Item { get; set; }
    }
}
