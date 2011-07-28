using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using WorkiSiteWeb.Infrastructure;

namespace WorkiSiteWeb.Models
{
    [MetadataType(typeof(Visitor_Validation))]
    public partial class Visitor
    {

    }

    public class VisitorFormViewModel
    {
        #region Properties

        public Visitor Visitor { get; set; }
        public LogOnModel LogOn { get; set; }

        #endregion

        public VisitorFormViewModel()
        {
            Visitor = new Visitor();
            LogOn = new LogOnModel();
        }
       
    }

    [Bind(Exclude = "Id")]
    public class Visitor_Validation
    {
        [StringLength(100, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(WorkiResources.Validation.ValidationString))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(WorkiResources.Validation.ValidationString))]
        [Email(ErrorMessageResourceName = "PatternEmail", ErrorMessageResourceType = typeof(WorkiResources.Validation.ValidationString))]
        public string Email { get; set; }
    }
}
