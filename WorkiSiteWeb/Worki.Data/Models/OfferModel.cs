using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.Linq;

namespace Worki.Data.Models
{
	public class OfferFormViewModel
	{
		public OfferFormViewModel()
		{
			var offers = Localisation.GetOfferTypeDict(new List<LocalisationOffer> { LocalisationOffer.FreeArea });
			Offers = new SelectList(offers, "Key", "Value", LocalisationOffer.BuisnessLounge);
            Periods = new SelectList(Offer.GetPaymentPeriodTypes(), "Key", "Value", Offer.PaymentPeriod.Hour);
            PaymentTypes = new SelectList(Offer.GetPaymentTypeEnumTypes(), "Key", "Value", Offer.PaymentTypeEnum.Paypal);
            Currencies = new SelectList(Offer.GetCurrencyEnumTypes(), "Key", "Value", Offer.CurrencyEnum.Euro);
			Offer = new Offer();
		}

		public Offer Offer { get; set; }
		public SelectList Offers { get; set; }
        public SelectList Periods { get; set; }
        public SelectList PaymentTypes { get; set; }
        public SelectList Currencies { get; set; }
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
	public partial class Offer : IPictureDataProvider, IFeatureProvider
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

		#region IFeatureProvider

        public string GetPrefix()
        {
            return FeatureHelper.OfferPrefix;
        }

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

		#region Booking Possibility

		public static bool CanHaveProduct(LocalisationOffer type)
		{
			return CanHaveBooking(type) || CanHaveQuotation(type);
		}

		public bool CanHaveProduct()
		{
			return CanHaveProduct((LocalisationOffer)Type);
		}

		public static bool CanHaveQuotation(LocalisationOffer type)
		{
			return type == LocalisationOffer.Desktop;
		}

		public bool CanHaveQuotation()
		{
			return CanHaveQuotation((LocalisationOffer)Type);
		}

		public static bool CanHaveBooking(LocalisationOffer type)
		{
			return	type == LocalisationOffer.Workstation ||
					type == LocalisationOffer.MeetingRoom ||
					type == LocalisationOffer.SeminarRoom ||
					type == LocalisationOffer.VisioRoom;
		}

		public bool CanHaveBooking()
		{
			return CanHaveBooking((LocalisationOffer)Type);
		}

		#endregion


        #region Payment

        /// <summary>
        /// Payment period
        /// </summary>
        public enum PaymentPeriod
        {
            Hour,
            Day,
            Week,
            Month,
            Year
        }

        public enum CurrencyEnum
        {
            Euro
        }

        public enum PaymentTypeEnum
        {
            Paypal
        }

        public static List<int> PaymentPeriodTypes = new List<int>()
        {
            (int)PaymentPeriod.Hour,
            (int)PaymentPeriod.Day,
            (int)PaymentPeriod.Week,
            (int)PaymentPeriod.Month,
            (int)PaymentPeriod.Year
        };

        public static List<int> CurrencyEnumTypes = new List<int>()
        {
            (int)CurrencyEnum.Euro
        };

        public static List<int> PaymentTypeEnumTypes = new List<int>()
        {
            (int)PaymentTypeEnum.Paypal
        };

        public static string GetPaymentPeriodType(int type)
        {
            var enumType = (PaymentPeriod)type;
            switch (enumType)
            {
                case PaymentPeriod.Hour:
                    return Worki.Resources.Models.Offer.Offer.Hour;
                case PaymentPeriod.Day:
                    return Worki.Resources.Models.Offer.Offer.Day;
                case PaymentPeriod.Week:
                    return Worki.Resources.Models.Offer.Offer.Week;
                case PaymentPeriod.Month:
                    return Worki.Resources.Models.Offer.Offer.Month;
                case PaymentPeriod.Year:
                    return Worki.Resources.Models.Offer.Offer.Year;
                default:
                    return string.Empty;
            }
        }

        public static string GetCurrencyEnumType(int type)
        {
            var enumType = (CurrencyEnum)type;
            switch (enumType)
            {
                case CurrencyEnum.Euro:
                    return Worki.Resources.Models.Offer.Offer.Euro;
                default:
                    return string.Empty;
            }
        }

        public static string GetPaymentTypeEnumType(int type)
        {
            var enumType = (PaymentTypeEnum)type;
            switch (enumType)
            {
                case PaymentTypeEnum.Paypal:
                    return Worki.Resources.Models.Offer.Offer.Paypal;
                default:
                    return string.Empty;
            }
        }

        public static Dictionary<int, string> GetPaymentPeriodTypes()
        {
            return PaymentPeriodTypes.ToDictionary(p => p, p => GetPaymentPeriodType(p));
        }

        public static Dictionary<int, string> GetCurrencyEnumTypes()
        {
            return CurrencyEnumTypes.ToDictionary(p => p, p => GetCurrencyEnumType(p));
        }

        public static Dictionary<int, string> GetPaymentTypeEnumTypes()
        {
            return PaymentTypeEnumTypes.ToDictionary(p => p, p => GetPaymentTypeEnumType(p));
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

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Price", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public decimal Price { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Period", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int Period { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "IsOnline", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public bool IsOnline { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "IsBookable", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public bool IsBookable { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "PaymentType", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int PaymentType { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Currency", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int Currency { get; set; }
	}

	public partial class OfferFeature : IFeatureContainer
	{
		public Feature Feature
		{
			get { return (Feature)FeatureId; }
		}
	}
}
