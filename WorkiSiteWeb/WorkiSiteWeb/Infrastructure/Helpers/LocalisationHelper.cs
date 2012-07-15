using System;
using System.Web;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Worki.Web.Helpers
{
    public static class LocalisationHelper
    {
        public static LocalisationJson GetJson(this Localisation localisation, Controller controller)
		{
            //get data from model
            var json = localisation.GetJson();

            //get url
            var urlHelper = new UrlHelper(controller.ControllerContext.RequestContext);
            json.url = urlHelper.Action(MVC.Localisation.ActionNames.Details, MVC.Localisation.Name, new { id = json.id, name = ControllerHelpers.GetSeoTitle(json.name), area = "" }, "http");

            //get image
            var image = localisation.LocalisationFiles.Where(f => f.IsDefault == true).FirstOrDefault();
            var imageUrl = image == null ? string.Empty : ControllerHelpers.GetUserImagePath(image.FileName, true);
			if (!string.IsNullOrEmpty(imageUrl) && VirtualPathUtility.IsAppRelative(imageUrl))
				json.image = ControllerHelpers.ResolveServerUrl(VirtualPathUtility.ToAbsolute(imageUrl), true);
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
    }

}