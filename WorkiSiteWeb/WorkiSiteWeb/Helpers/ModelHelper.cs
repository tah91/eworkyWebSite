using System;
using System.Web;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Worki.Web.Helpers
{
    public static class ModelHelper
    {
		public static LocalisationJson GetJson(this Localisation loc)
		{
			var image = loc.LocalisationFiles.Where(f => f.IsDefault == true).FirstOrDefault();
            var imageUrl = image == null ? string.Empty : ControllerHelpers.GetUserImagePath(image.FileName, true);
			return new LocalisationJson
			{
				ID = loc.ID,
				Latitude = loc.Latitude,
				Longitude = loc.Longitude,
				Name = loc.Name,
				Description = loc.Description,
				MainPic = imageUrl,
				Address = loc.Adress,
				City = loc.City,
				TypeString = Localisation.LocalisationTypes[loc.TypeValue]
			};
		}

        public static MetaData GetMetaData(this IPictureDataProvider provider)
        {
            if (provider == null)
                return null;

            var imageUrl = provider.GetMainPic();
            var imagePath = !string.IsNullOrEmpty(imageUrl) ?   ControllerHelpers.ResolveServerUrl(VirtualPathUtility.ToAbsolute(ControllerHelpers.GetUserImagePath(imageUrl, true)), false) :
                                                                ControllerHelpers.ResolveServerUrl(VirtualPathUtility.ToAbsolute(ControllerHelpers.GetUserImagePath(Links.Content.images.worki_fb_jpg, true)), false);

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

            return urlHelper.Action(MVC.Localisation.ActionNames.Details, MVC.Localisation.Name, new { id = loc.ID, name = ControllerHelpers.GetSeoTitle(loc.Name) }, "http");
        }

        public static string GetDetailFullUrl(this Rental rental, UrlHelper urlHelper)
        {
            if (rental == null || urlHelper == null)
                return null;

            return urlHelper.Action(MVC.Rental.ActionNames.Detail, MVC.Rental.Name, new { id = rental.Id }, "http");
        }
    }
}