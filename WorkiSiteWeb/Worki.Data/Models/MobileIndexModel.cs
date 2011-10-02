
namespace Worki.Data.Models
{
	public class MobileIndexModel
	{
		public SearchCriteriaFormViewModel SearchCriteriaFormViewModel;
		public LogOnModel LogOnModel;

		public MobileIndexModel()
		{
			SearchCriteriaFormViewModel = new SearchCriteriaFormViewModel();
			LogOnModel = new LogOnModel();
		}
	}
}
