using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using Worki.Web.Helpers;

namespace Worki.Web.ModelBinder
{
	public class LocalisationBinder : IModelBinder
	{
		private readonly IModelBinder _Binder;

		//prefixs for localisation gallery form
		public const string HiddenImagePrefix = "HiddenImg_";
		public const string IsDefaultPrefix = "IsDefault_";
		public const string IsLogoPrefix = "IsLogo_";

		public LocalisationBinder(IModelBinder binder)
		{
			_Binder = binder;
		}

		/// <summary>
		/// custom binding of localisation model, because tw things to handle manually
		///  - the features : LocalisationFeatures member to fill with checkboxs from form
		///  - the pictures : LocalisationFiles member to fill with hidden fields of images, and two radio buttons
		/// </summary>
		/// <param name="controllerContext">contains the form data to fill model</param>
		/// <param name="bindingContext">to add modelerrors if needed</param>
		/// <returns></returns>
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var loc = _Binder.BindModel(controllerContext, bindingContext) as Localisation;
			if (loc == null)
				return null;

			//handle features
			var keys = controllerContext.HttpContext.Request.Form.AllKeys;
			foreach (var key in keys)
			{
				KeyValuePair<int, int> toFill;
				if (!Localisation.GetFeatureDesc(key, out  toFill))
					continue;
				foreach (var feature in loc.LocalisationFeatures.ToList())
				{
					if (feature.FeatureID == toFill.Key && feature.OfferID == toFill.Value)
					{
						loc.LocalisationFeatures.Remove(feature);
					}
				}
				var hasFeature = controllerContext.HttpContext.Request.Form[key] as string;
				if (!string.IsNullOrEmpty(hasFeature) && hasFeature.ToLowerInvariant().Contains("true"))
				{
					var feature = new LocalisationFeature { FeatureID = toFill.Key, OfferID = toFill.Value };
					loc.LocalisationFeatures.Add(feature);
				}
			}
			if (bindingContext.ModelName == "localisation" && !loc.HasOffer())
			{
				bindingContext.ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.GiveMinADate);
			}

			//handle images
			loc.LocalisationFiles.Clear();
			var imageKeys = from item in keys where item.Contains(HiddenImagePrefix) select item;
			var defaultName = controllerContext.HttpContext.Request.Form[IsDefaultPrefix] as string;
			var logoName = controllerContext.HttpContext.Request.Form[IsLogoPrefix] as string;
			foreach (var key in imageKeys)
			{
				var value = controllerContext.HttpContext.Request.Form[key] as string;
				if (string.IsNullOrEmpty(value) || !value.Contains(HiddenImagePrefix))
					continue;
				var fileName = value.Replace(HiddenImagePrefix, string.Empty);
				loc.LocalisationFiles.Add(new LocalisationFile
				{
					FileName = fileName,
					IsDefault = string.Compare(defaultName, value) == 0,
					IsLogo = string.Compare(logoName, value) == 0
				});
			}

			return loc;
		}
	}

}