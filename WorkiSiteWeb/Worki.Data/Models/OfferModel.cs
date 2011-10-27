using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.Linq;

namespace Worki.Data.Models
{
	public static class FeatureHelper
	{
		public enum FeatureField
		{
			Bool,
			String,
			Number
		}

		public static FeatureField GetFieldType(Feature feature)
		{
			var intVal = (int)feature;
			if (intVal < 1000)
				return FeatureField.Bool;
			else if (intVal < 2000)
				return FeatureField.String;
			else
				return FeatureField.Number;
		}

		public const string LocalisationPrefix = "f_";
		public const string OfferPrefix = "o_";

		/// <summary>
		/// return a string for the feature
		/// to put in url query string
		/// </summary>
		/// <param name="featureId">the feature id</param>
		/// <param name="prefix">the prefix</param>
		/// <returns>a string</returns>
		public static string FeatureToString(int featureId, string prefix)
		{
			return prefix + featureId.ToString();
		}

		/// <summary>
		/// get the feature id from the string of query string
		/// </summary>
		/// <param name="featureStr">the string to recover</param>
		/// <param name="prefix">the prefix</param>
		/// <returns>the feature id</returns>
		public static int FeatureFromString(string featureStr, string prefix)
		{
			var toRet = -1;
			if (string.IsNullOrEmpty(featureStr))
				return toRet;
			var idStr = featureStr.Replace(prefix, string.Empty);
			try
			{
				toRet = int.Parse(idStr);
			}
			catch (Exception)
			{

			}
			return toRet;
		}

		/// <summary>
		/// convert a list of feature url to the corresponding feature ids
		/// </summary>
		/// <param name="strings">the feature urls</param>
		/// <returns>list of feature ids</returns>
		public static IEnumerable<int> GetFeatureIds(List<string> strings, string prefix)
		{
			foreach (var item in strings)
			{
				var id = FeatureFromString(item, prefix);
				if (id != -1)
					yield return id;
			}
		}
	}

	/// <summary>
	/// interface for handling features (for localisation and offer)
	/// </summary>
	public interface IFeatureContainer
	{
		/// <summary>
		/// Tell if it has a given feature
		/// </summary>
		/// <param name="feature">feature to check</param>
		/// <returns>true if have it</returns>
		bool HasFeature(Feature feature);

		/// <summary>
		/// Get the string value of a feature
		/// </summary>
		/// <param name="feature">the feature to check</param>
		/// <returns>string value, null if don't have the feature</returns>
		string GetStringFeature(Feature feature);

		/// <summary>
		/// Get the decimal value of a feature
		/// </summary>
		/// <param name="feature">the feature to check</param>
		/// <returns>decimal value, null if don't have the feature</returns>
		decimal? GetNumberFeature(Feature feature);
	}

	public class OfferFormViewModel
	{
		public OfferFormViewModel()
		{
			var offers = Localisation.GetOfferTypeDict(new List<LocalisationOffer> { LocalisationOffer.FreeArea });
			Offers = new SelectList(offers, "Key", "Value", LocalisationOffer.BuisnessLounge);
			Offer = new Offer();
		}

		public Offer Offer { get; set; }
		public SelectList Offers { get; set; }
	}

	public class OfferFeatureEqualityComparer : IEqualityComparer<OfferFeature>
	{
		#region IEqualityComparer<OfferFeature> Members

		public bool Equals(OfferFeature x, OfferFeature y)
		{
			return x.FeatureId == y.FeatureId;
		}

		public int GetHashCode(OfferFeature obj)
		{
			return base.GetHashCode();
		}

		#endregion
	}

	[MetadataType(typeof(Offer_Validation))]
	public partial class Offer : IPictureDataProvider, IFeatureContainer
	{
		#region IPictureDataProvider

		public int GetId()
		{
			return Id;
		}

		public ProviderType GetProviderType()
		{
			return ProviderType.Offer;
		}

		public List<PictureData> GetPictureData()
		{
			if (OfferFiles != null)
				return (from item in OfferFiles select new PictureData { FileName = item.FileName, IsDefault = item.IsDefault }).ToList();
			return new List<PictureData>();
		}

		public string GetMainPic()
		{
			var main = (from item in OfferFiles where item.IsDefault orderby item.Id select item.FileName).FirstOrDefault();
			return main;
		}

		public string GetPic(int index)
		{
			var list = (from item in OfferFiles where !item.IsDefault orderby item.Id select item.FileName).ToList();
			var count = list.Count();
			if (count == 0 || index < 0 || index >= count)
				return string.Empty;
			return list[index];
		}

		public string GetLogoPic()
		{
			throw new NotImplementedException("GetLogoPic");
		}

		public string GetDisplayName()
		{
			return Name;
		}

		public string GetDescription()
		{
			throw new NotImplementedException("GetDescription");
		}

		#endregion

		#region IFeatureContainer

		public bool HasFeature(Feature feature)
		{
			var equalityComparer = new OfferFeatureEqualityComparer();
			return OfferFeatures.Contains(new OfferFeature { FeatureId = (int)feature }, equalityComparer);
		}

		public string GetStringFeature(Feature feature)
		{
			var obj = OfferFeatures.FirstOrDefault(o => o.FeatureId == (int)feature);
			if (obj == null)
				return null;
			return obj.StringValue;
		}

		public decimal? GetNumberFeature(Feature feature)
		{
			var obj = OfferFeatures.FirstOrDefault(o => o.FeatureId == (int)feature);
			if (obj == null)
				return null;
			return obj.DecimalValue;
		}

		#endregion
	}

	[Bind(Exclude = "Id,LocalisationId")]
	public class Offer_Validation
	{
        [SelectValidation(ErrorMessageResourceName = "SelectOne", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Type", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
		public int Type { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Name", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
		public string Name { get; set; }
	}
}
