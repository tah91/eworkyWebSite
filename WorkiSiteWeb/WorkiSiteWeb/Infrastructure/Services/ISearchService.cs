using System;
using Worki.Web.Models;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Collections.Specialized;
using System.Web.Routing;

namespace Worki.Web.Infrastructure.Repository
{
	public interface ISearchService
	{
		SearchCriteriaFormViewModel GetCurrentSearchCriteria(NameValueCollection parameters);
		void FillResults(SearchCriteriaFormViewModel criteriaViewModel);
		SearchCriteria GetCriteria(NameValueCollection parameters);
		RouteValueDictionary GetRVD(SearchCriteria criteria, int page = 1);
		SearchSingleResultViewModel GetSingleResult(NameValueCollection parameters, int index);
		void ValidateLocalisation(Localisation toValidate, ref string error);
	}
}
