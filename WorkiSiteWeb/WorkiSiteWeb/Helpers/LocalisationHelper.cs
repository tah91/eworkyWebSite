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
		public static LocalisationJson GetJson(this Localisation loc)
		{
			var image = loc.LocalisationFiles.Where(f => f.IsDefault == true).FirstOrDefault();
			var imageUrl = image == null ? string.Empty : ControllerHelpers.GetUserImagePath(image.FileName);
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
    }

}