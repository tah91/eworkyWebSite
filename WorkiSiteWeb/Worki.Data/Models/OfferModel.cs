using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.Linq;
using Worki.Infrastructure.Helpers;

namespace Worki.Data.Models
{
	public class OfferFormViewModel
	{
		public OfferFormViewModel()
		{
            Init();
		}

		public OfferFormViewModel(bool isShared)
		{
			Init(isShared);
		}

		void Init(bool isShared = false)
        {
			var offers = Localisation.GetOfferTypeDict(isShared);
            Offers = new SelectList(offers, "Key", "Value", LocalisationOffer.AllOffers);
            Periods = new SelectList(Offer.GetPaymentPeriodTypes(), "Key", "Value", Offer.PaymentPeriod.Hour);
            PaymentTypes = new SelectList(Offer.GetPaymentTypeEnumTypes(), "Key", "Value", Offer.PaymentTypeEnum.Paypal);
            Currencies = new SelectList(Offer.GetCurrencyEnumTypes(), "Key", "Value", Offer.CurrencyEnum.EUR);
            IsSharedOffice = isShared;
            Offer = new Offer();
        }

		public Offer Offer { get; set; }
		public SelectList Offers { get; set; }
        public SelectList Periods { get; set; }
        public SelectList PaymentTypes { get; set; }
        public SelectList Currencies { get; set; }
		public int LocId { get; set; }
        public bool IsSharedOffice { get; set; }
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
		partial void OnInitialized()
		{
			IsOnline = true;
		}

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

        public static bool OfferCanHaveQuotation(LocalisationOffer type)
        {
            return type == LocalisationOffer.Desktop;
        }

        public static bool OfferCanHaveBooking(LocalisationOffer type)
        {
            return type == LocalisationOffer.Workstation ||
                    type == LocalisationOffer.MeetingRoom ||
                    type == LocalisationOffer.SeminarRoom ||
                    type == LocalisationOffer.VisioRoom;
        }

		/// <summary>
		/// True if offer can have quotation feature activated
		/// </summary>
		public bool CanHaveQuotation
		{
            get { return OfferCanHaveQuotation((LocalisationOffer)Type); }
		}

		/// <summary>
		/// True if offer can have booking feature activated
		/// </summary>
        public bool CanHaveBooking
        {
			get { return OfferCanHaveBooking((LocalisationOffer)Type) /*&& Localisation != null && !Localisation.IsSharedOffice()*/; }
        }

		/// <summary>
		/// True if offer has product feature activated
		/// </summary>
		public bool HasProduct
		{
			get { return IsBookable || IsQuotable; }
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
            Year,
			HalfDay
        }

        public enum CurrencyEnum
        {
            EUR,
            USD,
            GBP
        }

        public enum PaymentTypeEnum
        {
            Paypal,
            Check,
			Cash,
			Transfert
        }

        public static List<int> PaymentPeriodTypes = new List<int>()
        {
            (int)PaymentPeriod.Hour,
            (int)PaymentPeriod.HalfDay,
            (int)PaymentPeriod.Day,
            (int)PaymentPeriod.Week,
            (int)PaymentPeriod.Month,
            (int)PaymentPeriod.Year
        };

        public static List<int> CurrencyEnumTypes = new List<int>()
        {
            (int)CurrencyEnum.EUR,
            (int)CurrencyEnum.USD,
            (int)CurrencyEnum.GBP
        };

        public static List<int> PaymentTypeEnumTypes = new List<int>()
        {
            (int)PaymentTypeEnum.Paypal,
            (int)PaymentTypeEnum.Check,
			(int)PaymentTypeEnum.Cash,
			(int)PaymentTypeEnum.Transfert
        };

        public static string GetPaymentPeriodType(int type)
        {
            var enumType = (PaymentPeriod)type;
            switch (enumType)
            {
                case PaymentPeriod.Hour:
                    return Worki.Resources.Models.Offer.Offer.Hour;
                case PaymentPeriod.HalfDay:
                    return Worki.Resources.Models.Offer.Offer.HalfDay;
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
            return enumType.ToString();
        }

        public static string GetPaymentTypeEnumType(int type)
        {
            var enumType = (PaymentTypeEnum)type;
            switch (enumType)
            {
                case PaymentTypeEnum.Paypal:
                    return Worki.Resources.Models.Offer.Offer.Paypal;
                case PaymentTypeEnum.Check:
                    return Worki.Resources.Models.Offer.Offer.Check;
				case PaymentTypeEnum.Cash:
					return Worki.Resources.Models.Offer.Offer.Cash;
				case PaymentTypeEnum.Transfert:
					return Worki.Resources.Models.Offer.Offer.Transfert;
                default:
                    return string.Empty;
            }
        }

        public static Dictionary<int, string> GetPaymentPeriodTypes()
        {
            return PaymentPeriodTypes.ToDictionary(p => p, p => GetPaymentPeriodType(p));
        }

		public static Dictionary<int, string> GetPaymentPeriodTypes(IEnumerable<int> only)
		{
			var types = PaymentPeriodTypes.Where(t => only.Contains(t));
			return types.ToDictionary(p => p, p => GetPaymentPeriodType(p));
		}

        public static Dictionary<int, string> GetCurrencyEnumTypes()
        {
            return CurrencyEnumTypes.ToDictionary(p => p, p => GetCurrencyEnumType(p));
        }

        public static Dictionary<int, string> GetPaymentTypeEnumTypes()
        {
            return PaymentTypeEnumTypes.ToDictionary(p => p, p => GetPaymentTypeEnumType(p));
        }

		public static string GetPricingPeriod(PaymentPeriod period)
		{
			switch (period)
			{
				case PaymentPeriod.Hour:
					return Worki.Resources.Models.Offer.Offer.SingleHour;
				case PaymentPeriod.HalfDay:
					return Worki.Resources.Models.Offer.Offer.SingleHalfDay;
				case PaymentPeriod.Day:
					return Worki.Resources.Models.Offer.Offer.SingleDay;
				case PaymentPeriod.Week:
					return Worki.Resources.Models.Offer.Offer.SingleWeek;
				case PaymentPeriod.Month:
					return Worki.Resources.Models.Offer.Offer.SingleMonth;
				case PaymentPeriod.Year:
					return Worki.Resources.Models.Offer.Offer.SingleYear;
				default:
					return string.Empty;
			}
		}

        /// <summary>
        /// Get the default price for a given span
        /// </summary>
        /// <param name="span">span to price</param>
        /// <returns>default price</returns>
        public decimal GetDefaultPrice(DateTime startDate, DateTime endDate, bool useUnits, PaymentPeriod periodType, int periodUnits)
        {
            if (useUnits)
            {
                var offerPrice = OfferPrices.FirstOrDefault(op => op.PriceType == (int)periodType);
                if (offerPrice == null)
                    return 0;

                return periodUnits * offerPrice.Price;
            }
            else
            {
                //only happens for days
                var offerPrice = OfferPrices.FirstOrDefault(op => op.PriceType == (int)PaymentPeriod.Day);
                if (offerPrice == null)
                    return 0;

                var days = (endDate - startDate).TotalDays;
                return (decimal)(days * (double)offerPrice.Price);
            }
        }

		public IEnumerable<int> GetPricePeriods()
		{
			return OfferPrices.ToLookup(op => op.PriceType).Select(g => g.Key);
		}

        /// <summary>
        /// Get the min price of the offer, empty if no price
        /// </summary>
        /// <returns>the min price string</returns>
        public string GetMinPrice()
        {
            if (OfferPrices.Count() == 0)
                return string.Empty;

            var minPrice = OfferPrices.Min();
            if (minPrice == null)
                return string.Empty;

            return string.Format(Worki.Resources.Models.Offer.Offer.PriceFrom, minPrice.GetPriceDisplay());
        }

        #endregion

		#region Availability

		public static string GetAvailabilityPeriod(PaymentPeriod period)
		{
			switch (period)
			{
				case PaymentPeriod.Hour:
					return Worki.Resources.Models.Offer.Offer.PluralHour;
                case PaymentPeriod.HalfDay:
                    return Worki.Resources.Models.Offer.Offer.PluralHour;
				case PaymentPeriod.Day:
                    return Worki.Resources.Models.Offer.Offer.PluralDay;
				case PaymentPeriod.Week:
                    return Worki.Resources.Models.Offer.Offer.PluralWeek;
				case PaymentPeriod.Month:
                    return Worki.Resources.Models.Offer.Offer.PluralMonth;
				case PaymentPeriod.Year:
                    return Worki.Resources.Models.Offer.Offer.PluralYear;
				default:
					return string.Empty;
			}
		} 

        /// <summary>
        /// get availability display for an offer
        /// </summary>
        /// <param name="from">true if the availability is not exacte (aggregation of offers)</param>
        /// <returns>the availability display</returns>
		public string GetAvailabilityDisplay(bool from = false)
		{
			if (!Localisation.IsSharedOffice())
				return string.Empty;

            var toRet = string.Empty;
            if (AvailabilityDate.HasValue)
            {
                if (AvailabilityPeriod != 0)
                {
                    var format = from ? Worki.Resources.Models.Offer.Offer.AvailableFromFor : Worki.Resources.Models.Offer.Offer.AvailableAtFor;
                    toRet = string.Format(format, CultureHelpers.GetSpecificFormat(AvailabilityDate), AvailabilityPeriod, GetAvailabilityPeriod((PaymentPeriod)AvailabilityPeriodType));
                }
                else
                {
                    var format = from ? Worki.Resources.Models.Offer.Offer.AvailableFor : Worki.Resources.Models.Offer.Offer.AvailableAt;
                    toRet = string.Format(format, CultureHelpers.GetSpecificFormat(AvailabilityDate));
                }
            }
            else
            {
                if (AvailabilityPeriod != 0)
                {
                    toRet = string.Format(Worki.Resources.Models.Offer.Offer.AvailableFor, AvailabilityPeriod, GetAvailabilityPeriod((PaymentPeriod)AvailabilityPeriodType));
                }
            }

			return toRet;
		}

		#endregion

		#region Validation

		/// <summary>
		/// Validate offer, thow exception if not valid
		/// </summary>
		public void Validate()
		{
			string commentError = Worki.Resources.Validation.ValidationString.AlreadyRateThis;
			var group = OfferPrices.ToLookup(op => op.PriceType);
			foreach (var item in group)
			{
				if (item.Count() > 1)
					throw new Exception("Vous ne pouvez indiquer qu'un prix par periode");
			}
		}

        List<string> _StandardNames = new List<string> 
        {
            "Salon d'affaires 1", 
            "Poste de travail 1", 
            "Bureau 1", 
            "Salle de réunion / formation 1",
            "Salle de conférence / séminaire 1",
            "Salle de visio / téléprésence 1",
            "poste de travail",
            "salle de réunion",
            "poste de coworking",
            "Bureau",
            "Salon d'affaires",
            "salle de conférence",
            "salle de conférence/séminaire" 
        };

        public bool HasSpecificName()
        {
            return !_StandardNames.Contains(Name);   
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
        [Display(Name = "IsQuotable", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public bool IsQuotable { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "PaymentType", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int PaymentType { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Currency", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int Currency { get; set; }

        [Display(Name = "AvailabilityDate", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public DateTime? AvailabilityDate { get; set; }

        [Display(Name = "AvailabilityPeriod", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int AvailabilityPeriod { get; set; }

        [Display(Name = "AvailabilityPeriodType", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int AvailabilityPeriodType { get; set; }
	}

	public partial class OfferFeature : IFeatureContainer
	{
		public Feature Feature
		{
			get { return (Feature)FeatureId; }
		}
	}

	[MetadataType(typeof(OfferPrice_Validation))]
    public partial class OfferPrice : IComparable<OfferPrice>
	{
		/// <summary>
		/// Get price display : price / period
		/// </summary>
		/// <returns>the display</returns>
		public string GetPriceDisplay()
		{
			if (Price == 0)
				return string.Empty;

            var priceStr = Price.GetPriceDisplay((Offer.CurrencyEnum)Offer.Currency);
			return string.Format(Worki.Resources.Models.Offer.Offer.PricePerPeriod, priceStr, Offer.GetPricingPeriod((Offer.PaymentPeriod)PriceType));
		}

        public int CompareTo(OfferPrice other)
        {
            if (other.Price > this.Price)
                return -1;
            else if (other.Price == this.Price)
                return 0;
            else
                return 1;
        }
    }

	[Bind(Exclude = "Id,OfferId")]
	public class OfferPrice_Validation
	{
		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Price", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
		public decimal Price { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "PriceType", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
		public int PriceType { get; set; }
	}
}
