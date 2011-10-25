using System;
using System.Web;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace Worki.Web.Helpers
{
    public static class ModelHelper
	{
        public static string AbsoluteAction(this UrlHelper url, string action, string controller, object routeValues)
        {
            Uri requestUrl = url.RequestContext.HttpContext.Request.Url;

            var host = requestUrl.Authority;

            string absoluteAction = string.Format("{0}://{1}{2}",
                                                  requestUrl.Scheme,
                                                  host,
                                                  url.Action(action, controller, routeValues));

            return absoluteAction;
        }

		public static LocalisationJson GetJson(this Localisation localisation, Controller controller)
		{
			//get data from model
			var json = localisation.GetJson();

			//get url
			var urlHelper = new UrlHelper(controller.ControllerContext.RequestContext);
            json.url = localisation.GetDetailFullUrl(urlHelper);

			//get image
			var image = localisation.LocalisationFiles.Where(f => f.IsDefault == true).FirstOrDefault();
			var imageUrl = image == null ? string.Empty : ControllerHelpers.GetUserImagePath(image.FileName, true);
			if (!string.IsNullOrEmpty(imageUrl) && VirtualPathUtility.IsAppRelative(imageUrl))
				json.image = WebHelper.ResolveServerUrl(VirtualPathUtility.ToAbsolute(imageUrl), true);
			else
				json.image = imageUrl;

			//get comments
			foreach (var item in localisation.Comments)
			{
				json.comments.Add(item.GetJson());
			}

			//get fans
			foreach (var item in localisation.FavoriteLocalisations)
			{
				json.fans.Add(item.Member.GetJson());
			}

			//get amenities
			foreach (var item in localisation.LocalisationFeatures)
			{
				json.amenities.Add(Localisation.LocalisationFeatureDict[(int)item.FeatureID]);
			}
			return json;
		}

        public static MetaData GetMetaData(this IPictureDataProvider provider)
        {
            if (provider == null)
                return null;

            var imageUrl = provider.GetMainPic();
            var imagePath = !string.IsNullOrEmpty(imageUrl) ?   ControllerHelpers.GetUserImagePath(imageUrl, true) :
                                                                ControllerHelpers.GetUserImagePath(Links.Content.images.worki_fb_jpg, true);

            if (!string.IsNullOrEmpty(imagePath) && VirtualPathUtility.IsAppRelative(imagePath))
				imagePath = WebHelper.ResolveServerUrl(VirtualPathUtility.ToAbsolute(imagePath), true);

            return new MetaData
            {
                Title = provider.GetDisplayName(),
                Image = imagePath,
                Description = provider.GetDisplayName()
            };
        }

        public static string GetDetailFullUrl(this Localisation loc, UrlHelper urlHelper)
        {
            if (loc == null || urlHelper==null)
                return null;

			var type = MiscHelpers.GetSeoString(Localisation.LocalisationTypes[loc.TypeValue]);
			var name = MiscHelpers.GetSeoString(loc.Name);
            return urlHelper.AbsoluteAction(MVC.Localisation.ActionNames.Details, MVC.Localisation.Name, new { type = type, id = loc.ID, name = name, area = "" });
        }

        public static string GetDetailFullUrl(this Rental rental, UrlHelper urlHelper)
        {
            if (rental == null || urlHelper == null)
                return null;

            return urlHelper.AbsoluteAction(MVC.Rental.ActionNames.Detail, MVC.Rental.Name, new { id = rental.Id });
        }

		public static RouteValueDictionary GetOrderCrit(this RouteValueDictionary rvd, eOrderBy order)
		{
			switch (order)
			{
				case eOrderBy.Distance:
					rvd["order"] = (int)eOrderBy.Distance;
					break;
				case eOrderBy.Rating:
					rvd["order"] = (int)eOrderBy.Rating;
					break;
				default:
					break;
			}
			return rvd;
		}
    }
}