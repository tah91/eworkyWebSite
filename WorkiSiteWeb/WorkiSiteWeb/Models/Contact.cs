using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WorkiSiteWeb.Infrastructure;

namespace WorkiSiteWeb.Models
{
    public class Contact
    {
        #region Properties

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(WorkiResources.Validation.ValidationString))]
        [Display(Name = "LastName", ResourceType = typeof(WorkiResources.Models.Contact.Contact))]
        public string LastName { get; set; }

        [Display(Name = "FirstName", ResourceType = typeof(WorkiResources.Models.Contact.Contact))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(WorkiResources.Validation.ValidationString))]
        [Display(Name = "Email", ResourceType = typeof(WorkiResources.Models.Contact.Contact))]
        [Email(ErrorMessageResourceName = "PatternEmail", ErrorMessageResourceType = typeof(WorkiResources.Validation.ValidationString))]
        public string EMail { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(WorkiResources.Validation.ValidationString))]
        [Display(Name = "Subject", ResourceType = typeof(WorkiResources.Models.Contact.Contact))]
        public string Subject { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(WorkiResources.Validation.ValidationString))]
        public string Message { get; set; }

        #endregion
    }
}
