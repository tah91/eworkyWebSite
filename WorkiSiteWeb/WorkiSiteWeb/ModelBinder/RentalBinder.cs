using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using Worki.Web.Helpers;
using System;

namespace Worki.Web.ModelBinder
{
	public class RentalBinder : IModelBinder
	{
		private readonly IModelBinder _Binder;

		public RentalBinder(IModelBinder binder)
		{
			_Binder = binder;
		}

		/// <summary>
		/// custom binding of rental model, because three things to handle manually
		///  - the features : RentalFeatures member to fill with checkboxs from form
		///  - the pictures : RentalFiles member to fill with hidden fields of images, and one radio button
		///  - the access : RentalAccess member to fill with form list items
		/// </summary>
		/// <param name="controllerContext">contains the form data to fill model</param>
		/// <param name="bindingContext">to add modelerrors if needed</param>
		/// <returns></returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var rental = _Binder.BindModel(controllerContext, bindingContext) as Rental;
            if (rental == null)
                return null;

            //handle features
            var keys = controllerContext.HttpContext.Request.Form.AllKeys;
            foreach (var key in keys)
            {
                var parsedEnum = RentalFeatureType.Quiet;
                if (!Enum.TryParse<RentalFeatureType>(key, true, out parsedEnum))
                    continue;
                foreach (var feature in rental.RentalFeatures.ToList())
                {
                    if (feature.FeatureId == (int)parsedEnum)
                    {
                        rental.RentalFeatures.Remove(feature);
                    }
                }
                var hasFeature = controllerContext.HttpContext.Request.Form[key] as string;
                if (!string.IsNullOrEmpty(hasFeature) && hasFeature.ToLowerInvariant().Contains("true"))
                {
                    var feature = new RentalFeature { FeatureId = (int)parsedEnum };
                    rental.RentalFeatures.Add(feature);
                }
            }

            //handle images
            rental.RentalFiles.Clear();
            var imageKeys = from item in keys where item.Contains(PictureData.HiddenImagePrefix) select item;
            var defaultName = controllerContext.HttpContext.Request.Form[PictureData.IsDefaultPrefix] as string;
            foreach (var key in imageKeys)
            {
                var value = controllerContext.HttpContext.Request.Form[key] as string;
                if (string.IsNullOrEmpty(value) || !value.Contains(PictureData.HiddenImagePrefix))
                    continue;
                var fileName = value.Replace(PictureData.HiddenImagePrefix, string.Empty);
                rental.RentalFiles.Add(new RentalFile
                {
                    FileName = fileName,
                    IsDefault = string.Compare(defaultName, value) == 0
                });
            }

            return rental;
        }
	}

}