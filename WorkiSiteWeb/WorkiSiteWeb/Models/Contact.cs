using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Worki.Web.Infrastructure;

namespace Worki.Web.Models
{
    public class Contact
    {
        #region Properties

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "LastName", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        public string LastName { get; set; }

        [Display(Name = "FirstName", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Email", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        [Email(ErrorMessageResourceName = "PatternEmail", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string EMail { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Subject", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        public string Subject { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Message { get; set; }

        #endregion
    }
}
