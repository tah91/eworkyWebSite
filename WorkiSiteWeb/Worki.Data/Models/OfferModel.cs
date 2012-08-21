using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.Linq;
using Worki.Infrastructure.Helpers;
using System.Threading;
using System.Text.RegularExpressions;

namespace Worki.Data.Models
{
	public class OfferFormViewModel
	{
		public OfferFormViewModel()
		{
            Init();
		}

        public OfferFormViewModel(bool isShared, LocalisationOffer offerType = LocalisationOffer.AllOffers, int currentNeed = 0)
        {
            Init(isShared, offerType, currentNeed);
        }

        void Init(bool isShared = false, LocalisationOffer offerType = LocalisationOffer.Desktop, int currentNeed = 0)
        {
            var offers = Localisation.GetOfferTypeDict(isShared);
            Offers = new SelectList(offers, "Key", "Value", LocalisationOffer.AllOffers);
            Periods = new SelectList(Offer.GetPaymentPeriodTypes(), "Key", "Value", Offer.PaymentPeriod.Hour);
            PaymentTypes = new SelectList(Offer.GetPaymentTypeEnumTypes(), "Key", "Value", Offer.PaymentTypeEnum.Paypal);
            Currencies = new SelectList(Offer.GetCurrencyEnumTypes(), "Key", "Value", Offer.CurrencyEnum.EUR);
            ProductTypes = new SelectList(Offer.GetProductTypes(), "Key", "Value", Offer.eProductType.Quotation);
            IsSharedOffice = isShared;
            Offer = new Offer();
            if (offerType != LocalisationOffer.AllOffers)
                Offer.Type = (int)offerType;
            Offer.AllInclusive = true;
            var rangeEnd = currentNeed > 0 ? currentNeed : 1;
            DuplicateCountSelector = new SelectList(Enumerable.Range(0, rangeEnd).ToDictionary(k => k, k => k), "Key", "Value");
        }

		public Offer Offer { get; set; }
		public SelectList Offers { get; set; }
        public SelectList Periods { get; set; }
        public SelectList PaymentTypes { get; set; }
        public SelectList Currencies { get; set; }
        public SelectList ProductTypes { get; set; }
        public SelectList DuplicateCountSelector { get; set; }

        [Display(Name = "DuplicateCount", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int DuplicateCount { get; set; }
		public int LocId { get; set; }
        public bool IsSharedOffice { get; set; }
	}

    public class OfferCounterModel
    {
        void Initialize()
        {
            OfferLists = new Dictionary<LocalisationOffer, IList<Offer>>();
        }

        public OfferCounterModel()
        {
            Initialize();
        }

        public OfferCounterModel(Localisation loc)
        {
            Initialize();

            RefreshData(loc);

            if (OfferLists.ContainsKey(LocalisationOffer.BuisnessLounge))
                BuisnessLoungeCount = OfferLists[LocalisationOffer.BuisnessLounge].Count;
            if (OfferLists.ContainsKey(LocalisationOffer.Workstation))
                WorkstationCount = OfferLists[LocalisationOffer.Workstation].Count;
            if (OfferLists.ContainsKey(LocalisationOffer.Desktop))
                DesktopCount = OfferLists[LocalisationOffer.Desktop].Count;
            if (OfferLists.ContainsKey(LocalisationOffer.MeetingRoom))
                MeetingRoomCount = OfferLists[LocalisationOffer.MeetingRoom].Count;
            if (OfferLists.ContainsKey(LocalisationOffer.SeminarRoom))
                SeminarRoomCount = OfferLists[LocalisationOffer.SeminarRoom].Count;
            if (OfferLists.ContainsKey(LocalisationOffer.VisioRoom))
                VisioRoomCount = OfferLists[LocalisationOffer.VisioRoom].Count;

        }

        public Object GetJson()
        {
            return new
            {
                buisnessLoungeCount = BuisnessLoungeCount,
                workstationCount = WorkstationCount,
                desktopCount = DesktopCount,
                meetingRoomCount = MeetingRoomCount,
                seminarRoomCount = SeminarRoomCount,
                visioRoomCount = VisioRoomCount
            };
        }

        public void RefreshData(Localisation loc)
        {
            Localisation = loc;
            IsSharedOffice = loc.IsSharedOffice();
            var groupedOffers = (from item
                                     in loc.Offers
                                 group item by item.Type into grp
                                 where grp.Count() > 0
                                 select grp);

            foreach (var item in groupedOffers)
            {
                OfferLists.Add((LocalisationOffer)item.Key, item.ToList());
            }
        }

        [Display(Name = "BuisnessLoungeCount", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int BuisnessLoungeCount { get; set; }

        [Display(Name = "WorkstationCount", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int WorkstationCount { get; set; }

        [Display(Name = "DesktopCount", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int DesktopCount { get; set; }

        [Display(Name = "MeetingRoomCount", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int MeetingRoomCount { get; set; }

        [Display(Name = "SeminarRoomCount", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int SeminarRoomCount { get; set; }

        [Display(Name = "VisioRoomCount", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int VisioRoomCount { get; set; }

        public Localisation Localisation { get; set; }
        public bool IsSharedOffice { get; set; }
        public EditionType EditionType { get; set; }
        public IDictionary<LocalisationOffer, IList<Offer>> OfferLists { get; set; }

        public bool NeedAddThisOffer(LocalisationOffer offerType, out int currentNeed, out string helpText)
        {
            currentNeed = 0;
            var offerTypeRequiredCount = 0;
            helpText = "";
            switch (offerType)
            {
                case LocalisationOffer.BuisnessLounge:
                    offerTypeRequiredCount = BuisnessLoungeCount;
                    helpText = Worki.Resources.Models.Offer.Offer.BuisnessLoungeTellMore;
                    break;
                case LocalisationOffer.Desktop:
                    offerTypeRequiredCount = DesktopCount;
                    helpText = Worki.Resources.Models.Offer.Offer.DesktopTellMore;
                    break;
                case LocalisationOffer.Workstation:
                    offerTypeRequiredCount = WorkstationCount;
                    helpText = Worki.Resources.Models.Offer.Offer.WorkstationTellMore;
                    break;
                case LocalisationOffer.MeetingRoom:
                    offerTypeRequiredCount = MeetingRoomCount;
                    helpText = Worki.Resources.Models.Offer.Offer.MeetingRoomTellMore;
                    break;
                case LocalisationOffer.SeminarRoom:
                    offerTypeRequiredCount = SeminarRoomCount;
                    helpText = Worki.Resources.Models.Offer.Offer.SeminarRoomTellMore;
                    break;
                case LocalisationOffer.VisioRoom:
                    offerTypeRequiredCount = VisioRoomCount;
                    helpText = Worki.Resources.Models.Offer.Offer.VisioRoomTellMore;
                    break;
                default:
                    break;
            }

            if (offerTypeRequiredCount <= 0)
                return false;

            if (!OfferLists.ContainsKey(offerType))
            {
                currentNeed = offerTypeRequiredCount;
                return true;
            }

            currentNeed = offerTypeRequiredCount - OfferLists[offerType].Count;
            return currentNeed > 0;
        }

        public bool NeedAddOffer(out LocalisationOffer offerType, out int currentNeed, out string helpText)
        {
            currentNeed = 0;
            helpText = "";
            offerType = LocalisationOffer.AllOffers;

            if (NeedAddThisOffer(LocalisationOffer.BuisnessLounge, out currentNeed, out helpText))
            {
                offerType = LocalisationOffer.BuisnessLounge;
                return true;
            }
            else if (NeedAddThisOffer(LocalisationOffer.Workstation, out currentNeed, out helpText))
            {
                offerType = LocalisationOffer.Workstation;
                return true;
            }
            else if (NeedAddThisOffer(LocalisationOffer.Desktop, out currentNeed, out helpText))
            {
                offerType = LocalisationOffer.Desktop;
                return true;
            }
            else if (NeedAddThisOffer(LocalisationOffer.MeetingRoom, out currentNeed, out helpText))
            {
                offerType = LocalisationOffer.MeetingRoom;
                return true;
            }
            else if (NeedAddThisOffer(LocalisationOffer.SeminarRoom, out currentNeed, out helpText))
            {
                offerType = LocalisationOffer.SeminarRoom;
                return true;
            }
            else if (NeedAddThisOffer(LocalisationOffer.VisioRoom, out currentNeed, out helpText))
            {
                offerType = LocalisationOffer.VisioRoom;
                return true;
            }

            return false;
        }
    }

    public class OfferFormListModelItem
    {
        public Offer Offer { get; set; }
        public bool IsSharedOffice { get; set; }

        public string GetDisplay()
        {
            var toRet = Offer.Name;
            var minPrice = Offer.OfferPrices.Min();
            if(minPrice==null)
                return toRet;

            return string.Format(Worki.Resources.Views.Offer.OfferString.OfferListDisplay, toRet, minPrice.GetPriceDisplay());
        }
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
	public partial class Offer : IPictureDataProvider, IFeatureProvider, IJsonProvider<OfferJson>, IValidatableObject
	{
		partial void OnInitialized()
		{
			IsOnline = true;
		}

        #region IJsonProvider

        public OfferJson GetJson()
        {
            var prices = new List<PriceJson>();
            foreach (var p in OfferPrices)
            {
                prices.Add(new PriceJson
                {
                    price = p.GetAbsolute(),
                    frequency = p.GetFrequency()
                });
            }

            var features = new List<FeatureJson>();
            foreach (var feature in OfferFeatures)
            {
                features.Add(new FeatureJson { featureId = feature.FeatureId, featureDisplay = FeatureHelper.Display(feature) });
            }

            return new OfferJson
            {
                id = Id,
                name = GetDisplayName(),
                availability = GetAvailabilityDisplay(),
                prices = prices,
                features = features,
                offerType = Type
            };
        }

        #endregion

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
            switch (Thread.CurrentThread.CurrentUICulture.Name)
            {
                case "en":
                    return DescriptionEn;
                case "es":
                    return DescriptionEs;
                case "de":
                    return DescriptionDe;
                case "fr":
                default:
                    return Description;
            }
        }

        public string GetDescriptionName()
        {
            switch (Thread.CurrentThread.CurrentUICulture.Name)
            {
                case "en":
                    return "DescriptionEn";
                case "es":
                    return "DescriptionEs";
                case "de":
                    return "DescriptionDe";
                default:
                    return "Description";
            }
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

        /// <summary>
        /// King of product this offer is
        /// </summary>
        public enum eProductType
        {
            Quotation,
            Booking
        }

        public static List<int> ProductTypes = new List<int>()
        {
            (int)eProductType.Quotation,
            (int)eProductType.Booking
        };

        public static string GetProductTypes(int type)
        {
            var enumType = (eProductType)type;
            switch (enumType)
            {
                case eProductType.Quotation:
                    return Worki.Resources.Models.Offer.Offer.Quotation;
                case eProductType.Booking:
                    return Worki.Resources.Models.Offer.Offer.Booking;
                default:
                    return string.Empty;
            }
        }

        public static Dictionary<int, string> GetProductTypes()
        {
            return ProductTypes.ToDictionary(p => p, p => GetProductTypes(p));
        }

        public bool AcceptBooking()
        {
            return ProductType == (int)eProductType.Booking;
        }

        public bool AcceptQuotation()
        {
            return ProductType == (int)eProductType.Quotation;
        }

		/// <summary>
		/// True if offer can be booked
		/// </summary>
		public bool IsReallyBookable()
		{
            return AcceptBooking() && OfferPrices != null && OfferPrices.Count != 0;
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
            GBP,
            BRL,
            CAD,
            CHF,
            CZK,
            DKK,
            HKD,
            HUF,
            ILS,
            INR,
            JPY,
            NOK,
            NZD,
            PLN,
            RUB,
            SEK,
            ZAR,
            AUD,
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
            (int)CurrencyEnum.GBP,
            (int)CurrencyEnum.BRL,
            (int)CurrencyEnum.CAD,
            (int)CurrencyEnum.CHF,
            (int)CurrencyEnum.CZK,
            (int)CurrencyEnum.DKK,
            (int)CurrencyEnum.HKD,
            (int)CurrencyEnum.HUF,
            (int)CurrencyEnum.ILS,
            (int)CurrencyEnum.INR,
            (int)CurrencyEnum.JPY,
            (int)CurrencyEnum.NOK,
            (int)CurrencyEnum.NZD,
            (int)CurrencyEnum.PLN,
            (int)CurrencyEnum.RUB,
            (int)CurrencyEnum.SEK,
            (int)CurrencyEnum.ZAR,
            (int)CurrencyEnum.AUD,
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

        /// <summary>
        /// Get the min price of the offer, empty if no price
        /// </summary>
        /// <returns>the min price string</returns>
        public string GetFirstPrice()
        {
            if (OfferPrices.Count() == 0)
                return string.Empty;

            var firstPrice = OfferPrices.FirstOrDefault();
            if (firstPrice == null)
                return string.Empty;

            return firstPrice.GetPriceDisplay();
        }

        public bool HasPriceOfType(int priceType)
        {
            return OfferPrices.Count(op => op.PriceType == priceType) > 0;
        }

        public bool ForceHasPriceOfType(int priceType)
        {
            if (OfferPrices.Count() > 0)
            {
                return HasPriceOfType(priceType);
            }
            else
            {
                return true;
            }
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

        #region IValidatableObject

        public const string PricesField = "PricesField";
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            string error = "";
            if (OfferPrices.Count < 1)
            {
                error = string.Format(Worki.Resources.Validation.ValidationString.OfferPriceType, Worki.Resources.Models.Offer.Offer.LeaseTerm);
                yield return new ValidationResult(error, new[] { PricesField });
            }

            foreach (OfferPrice offer in OfferPrices)
            {
                if (offer.Price > 0 == false)
                {
                    string inputName = Worki.Resources.Models.Offer.Offer.PriceBy + " " + Offer.GetPaymentPeriodType(offer.PriceType);
                    error = string.Format(Worki.Resources.Validation.ValidationString.SuperiorTo, inputName, 0);
                    yield return new ValidationResult(error, new[] { PricesField });
                }
            }

            if (string.IsNullOrEmpty(this.GetDescription()))
            {
                error = string.Format(Worki.Resources.Validation.ValidationString.Required, Worki.Resources.Models.Offer.Offer.Description);
                yield return new ValidationResult(error, new[] { GetDescriptionName() });
            }
            else
            {
                if (FormValidation.ValidateDescription(GetDescription()))
                {
                    error = string.Format(Worki.Resources.Validation.ValidationString.ProhibitedString, Worki.Resources.Models.Offer.Offer.Description);
                    yield return new ValidationResult(error, new[] { GetDescriptionName() });
                }
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

        #region Replication

        public IEnumerable<Offer> Replicate(int count)
        {
            var toRet = new List<Offer>();
            for (int index = 0; index < count; ++index)
            {
                var toAdd = new Offer
                {
                    Type = Type,
                    Name = Name + " " + (index + 2).ToString(),
                    Capacity = Capacity,
                    Price = Price,
                    Period = Period,
                    IsOnline = IsOnline,
                    IsBookable = IsBookable,
                    IsQuotable = IsQuotable,
                    PaymentType = PaymentType,
                    Currency = Currency,
                    AvailabilityDate = AvailabilityDate,
                    AvailabilityPeriod = AvailabilityPeriod,
                    AvailabilityPeriodType = AvailabilityPeriodType,
                    ProductType = ProductType,
                    Description = Description,
                    DescriptionDe = DescriptionDe,
                    DescriptionEn = DescriptionEn,
                    DescriptionEs = DescriptionEs,
                    AllInclusive = AllInclusive
                };

                foreach (var feature in OfferFeatures)
                {
                    toAdd.OfferFeatures.Add(new OfferFeature
                    {
                        FeatureId = feature.FeatureId,
                        StringValue = feature.StringValue,
                        DecimalValue = feature.DecimalValue
                    });
                }

                foreach (var file in OfferFiles)
                {
                    toAdd.OfferFiles.Add(new OfferFile
                    {
                        FileName = file.FileName,
                        IsDefault = file.IsDefault
                    });
                }

                foreach (var price in OfferPrices)
                {
                    toAdd.OfferPrices.Add(new OfferPrice
                    {
                        Price = price.Price,
                        PriceType = price.PriceType
                    });
                }

                toRet.Add(toAdd);
            }

            return toRet;
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

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "ProductType", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        public int ProductType { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Worki.Resources.Views.Offer.OfferString))]
        [StringLength(2000, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Description { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Worki.Resources.Views.Offer.OfferString))]
        [StringLength(2000, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string DescriptionEn { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Worki.Resources.Views.Offer.OfferString))]
        [StringLength(2000, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string DescriptionEs { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Worki.Resources.Views.Offer.OfferString))]
        [StringLength(2000, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string DescriptionDe { get; set; }

        [Display(Name = "AllInclusive", ResourceType = typeof(Worki.Resources.Models.Offer.Offer))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public bool AllInclusive { get; set; }
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

            return string.Format(Worki.Resources.Models.Offer.Offer.PricePerPeriod, GetAbsolute(), GetFrequency());
		}

        /// <summary>
        /// return price with currency but without frequency
        /// </summary>
        /// <returns></returns>
        public string GetAbsolute()
        {
            if (Price == 0)
                return "";

            return Price.GetPriceDisplay((Offer.CurrencyEnum)Offer.Currency, false);
        }

        /// <summary>
        /// return frequency
        /// </summary>
        /// <returns></returns>
        public string GetFrequency()
        {
            if (Price == 0)
                return "";

            return Offer.GetPricingPeriod((Offer.PaymentPeriod)PriceType);
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
