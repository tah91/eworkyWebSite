using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Reflection;
using WorkiSiteWeb.Helpers;
using System.Collections.Generic;
using WorkiSiteWeb.Models;

namespace WorkiSiteWeb.Models
{
	public class LocalisationBinder : IModelBinder
	{
		private readonly IModelBinder _Binder;
		public static Dictionary<int, string> LocalisationFeatureDict = MiscHelpers.GetEnumDescriptors(typeof(Feature));
		public static Dictionary<FeatureType, string> LocalisationFeatureTypes = new Dictionary<FeatureType, string>()
        {
            { FeatureType.General, string.Empty},
            { FeatureType.WorkingPlace, WorkiResources.Models.Localisation.Localisation.WorkingPlace},
            { FeatureType.MeetingRoom, WorkiResources.Models.Localisation.Localisation.MeetingRoom},
            { FeatureType.SeminarRoom, WorkiResources.Models.Localisation.Localisation.SeminarRoom},
            { FeatureType.VisioRoom, WorkiResources.Models.Localisation.Localisation.VisioRoom}
        };

		public static List<int> OfferTypes = new List<int>()
        {
            (int)Feature.BuisnessRoom,
            (int)Feature.Workstation,
            (int)Feature.MeetingRoom,
            (int)Feature.SeminarRoom,
            (int)Feature.VisioRoom,
            (int)Feature.SingleDesk,
            (int)Feature.FreeArea
        };

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
				if (!MiscHelpers.GetFeatureDesc(key, out  toFill))
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
				bindingContext.ModelState.AddModelError("", WorkiResources.Validation.ValidationString.GiveMinADate);
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