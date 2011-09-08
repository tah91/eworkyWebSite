using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Rest;
using Worki.Infrastructure.Logging;
using Worki.Services;
using Worki.Web.Helpers;

namespace Worki.Web.Areas.Api.Controllers
{
    public partial class LocalisationController : Controller
    {
        ILocalisationRepository _LocalisationRepository;
        IMemberRepository _MemberRepository;
        ILogger _Logger;
        ISearchService _SearchService;

        public LocalisationController(ILocalisationRepository localisationRepository, IMemberRepository memberRepository, ILogger logger, ISearchService searchService)
        {
            _LocalisationRepository = localisationRepository;
            _MemberRepository = memberRepository;
            _Logger = logger;
            _SearchService = searchService;
        }

        public virtual ActionResult Details(int id)
        {
            var localisation = _LocalisationRepository.Get(id);
            if (localisation == null)
                return new ObjectResult<List<LocalisationJson>>(null, 400, "The id is not present in database");

            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
            var json = localisation.GetJson();
            json.Url = urlHelper.Action(MVC.Localisation.ActionNames.Details, MVC.Localisation.Name, new { id = json.ID, name = ControllerHelpers.GetSeoTitle(json.Name), area = "" }, "http");
            json.MainPic = ControllerHelpers.ResolveServerUrl(VirtualPathUtility.ToAbsolute(json.MainPic), true);
            return new ObjectResult<LocalisationJson>(json);
        }
    }
}
