using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Worki.Infrastructure;

namespace Worki.Data.Models
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

        [Display(Name = "ToEmail", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        public string ToEMail { get; set; }

        [Display(Name = "Link", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        public string Link { get; set; }

        [Display(Name = "ToName", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        public string ToName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Subject", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        public string Subject { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Message { get; set; }

        #endregion
    }
}
