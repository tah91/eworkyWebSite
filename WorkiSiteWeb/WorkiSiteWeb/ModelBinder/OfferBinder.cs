using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using Worki.Web.Helpers;
using System;

namespace Worki.Web.ModelBinder
{
	public class OfferBinder : IModelBinder
	{
		private readonly IModelBinder _Binder;

		public OfferBinder(IModelBinder binder)
		{
			_Binder = binder;
		}

		/// <summary>
		/// custom binding of offer model, because two things to handle manually
		///  - the features : OfferFeatures member to fill with checkboxs from form
		///  - the pictures : OfferFiles member to fill with hidden fields of images, and one radio button
		/// </summary>
		/// <param name="controllerContext">contains the form data to fill model</param>
		/// <param name="bindingContext">to add modelerrors if needed</param>
		/// <returns></returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var offer = _Binder.BindModel(controllerContext, bindingContext) as Offer;
			if (offer == null)
                return null;

            //handle features
            var keys = controllerContext.HttpContext.Request.Form.AllKeys;
            var offerFeatureKeys = FeatureHelper.GetFeatureStrings(controllerContext.HttpContext.Request.Form.AllKeys.ToList(), FeatureHelper.OfferPrefix);
            foreach (var key in offerFeatureKeys)
			{
				Feature parsedEnum;
				if (!Enum.TryParse<Feature>(key, true, out parsedEnum))
					continue;
				foreach (var feature in offer.OfferFeatures.ToList())
				{
					if (feature.FeatureId == (int)parsedEnum)
					{
						offer.OfferFeatures.Remove(feature);
					}
				}
				var fieldType = FeatureHelper.GetFieldType(parsedEnum);
                var fieldValue = controllerContext.HttpContext.Request.Form[FeatureHelper.GetStringId(parsedEnum, FeatureHelper.OfferPrefix)] as string;
				if (string.IsNullOrEmpty(fieldValue))
					continue;

				switch (fieldType)
				{
					case FeatureHelper.FeatureField.String:
						{
							var feature = new OfferFeature { FeatureId = (int)parsedEnum, StringValue = fieldValue };
							offer.OfferFeatures.Add(feature);
							break;
						}
					case FeatureHelper.FeatureField.Number:
						{
							try
							{
								var numberFieldValue = decimal.Parse(fieldValue);
								var feature = new OfferFeature { FeatureId = (int)parsedEnum, DecimalValue = numberFieldValue };
								offer.OfferFeatures.Add(feature);
							}
							catch (Exception)
							{
								break;
							}
							break;
						}
					case FeatureHelper.FeatureField.Bool:
					default:
						{

							if (!string.IsNullOrEmpty(fieldValue) && fieldValue.ToLowerInvariant().Contains("true"))
							{
								var feature = new OfferFeature { FeatureId = (int)parsedEnum };
								offer.OfferFeatures.Add(feature);
							}
							break;
						}
				}
			}

            //handle images
			offer.OfferFiles.Clear();
            var imageKeys = from item in keys where item.Contains(PictureData.HiddenImagePrefix) select item;
            var defaultName = controllerContext.HttpContext.Request.Form[PictureData.IsDefaultPrefix] as string;
            foreach (var key in imageKeys)
            {
                var value = controllerContext.HttpContext.Request.Form[key] as string;
                if (string.IsNullOrEmpty(value) || !value.Contains(PictureData.HiddenImagePrefix))
                    continue;
                var fileName = value.Replace(PictureData.HiddenImagePrefix, string.Empty);
				offer.OfferFiles.Add(new OfferFile
                {
                    FileName = fileName,
                    IsDefault = string.Compare(defaultName, value) == 0
                });
            }

			return offer;
        }
	}

}