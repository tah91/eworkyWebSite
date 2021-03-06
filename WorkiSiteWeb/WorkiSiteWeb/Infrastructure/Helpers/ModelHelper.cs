﻿using System;
using System.Web;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Worki.Web.Model;
using Worki.Memberships;
using Worki.Infrastructure;
using System.Globalization;

namespace Worki.Web.Helpers
{
    public static class ModelHelper
	{
        public static string AbsoluteAction(this UrlHelper url, string action, string controller, object routeValues)
        {
            Uri requestUrl = url.RequestContext.HttpContext.Request.Url;

            var host = requestUrl.Authority;

            string absoluteAction = string.Format("{0}://{1}{2}",
                                                  requestUrl.Scheme,
                                                  host,
                                                  url.Action(action, controller, routeValues));

            return absoluteAction;
        }


        static string GetImageUrl(string image)
        {
            if (string.IsNullOrEmpty(image))
                return image;

            if (image.StartsWith("http://") || image.StartsWith("https://"))
                return image;

            return WebHelper.ResolveServerUrl(VirtualPathUtility.ToAbsolute(image), false);
        }

        public static OfferJson GetJson(this Offer offer)
        {
            //get data from model
            var json = offer.GetJson();

            //get images
            var images = offer.OfferFiles.OrderBy(f => f.IsDefault);
            foreach (var item in images)
            {
                json.images.Add(new ImageJson
                {
                    url = GetImageUrl(item == null ? string.Empty : ControllerHelpers.GetUserImagePath(item.FileName)),
                    thumbnail_url = GetImageUrl(item == null ? string.Empty : ControllerHelpers.GetUserImagePath(item.FileName))
                });
            }

            return json;
        }

		public static LocalisationJson GetJson(this Localisation localisation, Controller controller)
		{
			//get data from model
			var json = localisation.GetJson();

			//get url
			var urlHelper = new UrlHelper(controller.ControllerContext.RequestContext);
            json.url = localisation.GetDetailFullUrl(urlHelper);

			//get images
			var images = localisation.LocalisationFiles.OrderBy(f => f.IsDefault);
            foreach (var item in images)
            {
                json.images.Add(new ImageJson
                {
                    url = GetImageUrl(item == null ? string.Empty : ControllerHelpers.GetUserImagePath(item.FileName)),
                    thumbnail_url = GetImageUrl(item == null ? string.Empty : ControllerHelpers.GetUserImagePath(item.FileName, true))
                });
            }

            foreach (var item in localisation.GetOrderedComments(MultiCultureMvcRouteHandler.DefaultCulture))
            {
                var toAdd = item.GetJson();
                toAdd.author.avatar = new ImageJson
                {
                    url = GetImageUrl(item == null ? string.Empty : ControllerHelpers.GetUserImagePath(item.Member.MemberMainData.Avatar, false, Member.AvatarFolder)),
                    thumbnail_url = GetImageUrl(item == null ? string.Empty : ControllerHelpers.GetUserImagePath(item.Member.MemberMainData.Avatar, true, Member.AvatarFolder))
                };
                json.comments.Add(toAdd);
            }

			return json;
		}

        public static LocalisationJson GetJson(this Localisation localisation, Controller controller, SearchCriteria criteria)
        {
            var json = GetJson(localisation, controller);

            //get distance
            json.distance = MiscHelpers.GetDistanceBetween(criteria.LocalisationData.Latitude, criteria.LocalisationData.Longitude, localisation.Latitude, localisation.Longitude);
            return json;
        }

        public static AuthJson GetAuthData(this Member member)
        {
            var toRet = member.GetAuthJson();
            toRet.avatar = GetImageUrl(ControllerHelpers.GetUserImagePath(member.MemberMainData.Avatar, true, Member.AvatarFolder));

            return toRet;
        }

        public static AuthJson GetAuthData(IMembershipService service, string key)
        {
            var toRet = service.GetAuthData(key);
            toRet.avatar = GetImageUrl(ControllerHelpers.GetUserImagePath(toRet.avatar, true, Member.AvatarFolder));

            return toRet;
        }

        public static MetaData GetMetaData(this IPictureDataProvider provider, UrlHelper urlHelper)
        {
            if (provider == null)
                return null;

            var imageUrl = provider.GetMainPic();
			var imagePath = !string.IsNullOrEmpty(imageUrl) ?	ControllerHelpers.GetUserImagePath(imageUrl, true, PictureData.GetFolder(provider.GetProviderType())) :
																ControllerHelpers.GetUserImagePath(Links.Content.images.worki_fb_jpg, true);

            if (!string.IsNullOrEmpty(imagePath) && VirtualPathUtility.IsAppRelative(imagePath))
				imagePath = WebHelper.ResolveServerUrl(VirtualPathUtility.ToAbsolute(imagePath), true);

            var metaData = new MetaData
            {
                Title = provider.GetDisplayName(),
                Image = imagePath,
                Description = provider.GetDescription()
            };

            if (provider is Localisation)
            {
                var localisation = (Localisation)provider;
                metaData.CanonicalUrl = localisation.GetDetailFullUrl(urlHelper, MultiCultureMvcRouteHandler.DefaultCulture.ToString());
                foreach (var lang in MultiCultureMvcRouteHandler.Cultures)
                {
                    metaData.AlternateUrls.Add(new AlternateUrl { Lang = lang, Url = localisation.GetDetailFullUrl(urlHelper, lang) });
                }
            }

            return metaData;
        }

        public static string GetDetailFullUrl(this Localisation loc, UrlHelper urlHelper, string culture = null)
        {
            if (loc == null || urlHelper == null)
                return null;

            if(string.IsNullOrEmpty(culture))
            {
                culture = CultureInfo.CurrentCulture.ToString().Substring(0, 2);
            }

            string typeStr = Localisation.GetLocalisationSeoType(loc.TypeValue);
            var type = MiscHelpers.GetSeoString(typeStr);
            var name = MiscHelpers.GetSeoString(loc.GetFullName());
            return urlHelper.AbsoluteAction(MVC.Localisation.ActionNames.Details, MVC.Localisation.Name, new { type = type, id = loc.ID, name = name, area = "", culture = culture });
        }

        public static string GetDetailFullUrl(this Rental rental, UrlHelper urlHelper)
        {
            if (rental == null || urlHelper == null)
                return null;

            return urlHelper.AbsoluteAction(MVC.Rental.ActionNames.Detail, MVC.Rental.Name, new { id = rental.Id });
        }

		public static List<NewsItem> GetNews(int memberId, IEnumerable<MemberBooking> bookings, Func<MemberBookingLog, string> linkFunction)
		{
			var toRet = new List<NewsItem>();
			foreach (var booking in bookings)
			{
				foreach (var log in booking.MemberBookingLogs)
				{
					if (log.EventType == (int)MemberBookingLog.BookingEvent.General)
						continue;

					if (log.LoggerId == memberId)
						continue;

					toRet.Add(new NewsItem
					{
						Date = log.CreatedDate,
						DisplayName = log.GetDisplay(),
						Link = linkFunction(log),
						Read = log.Read
					});
				}
			}

			return toRet;
		}

        public static List<NewsItem> GetNews(int memberId, IEnumerable<MemberQuotation> quotations, Func<MemberQuotationLog, string> linkFunction)
        {
            var toRet = new List<NewsItem>();
            foreach (var quotation in quotations)
            {
                foreach (var log in quotation.MemberQuotationLogs)
                {
                    if (log.EventType == (int)MemberQuotationLog.QuotationEvent.General)
                        continue;

					if (log.LoggerId == memberId)
						continue;

                    toRet.Add(new NewsItem
                    {
                        Date = log.CreatedDate,
                        DisplayName = log.GetDisplay(),
						Link = linkFunction(log),
						Read = log.Read
                    });
                }
            }

            return toRet;
        }

		public static DropDownModel GetOfferDropDown(Offer offer, Func<Offer,string> urlMaker, OfferDropDownFilter filter = OfferDropDownFilter.None)
		{
			var dropDown = new DropDownModel
			{
				Id = DropDownModel.OfferDD,
				Title = offer.Name,
				Items = new List<DropDownItem>()
			};
            foreach (var item in offer.Localisation.Offers)
            {
                switch (filter)
                {
                    case OfferDropDownFilter.Quotation:
                        {
                            if (item.AcceptQuotation())
                                dropDown.Items.Add(new DropDownItem { DisplayName = item.Name, Link = urlMaker(item) });
                            break;
                        }
                    case OfferDropDownFilter.Booking:
                        {
                            if (item.AcceptBooking())
                                dropDown.Items.Add(new DropDownItem { DisplayName = item.Name, Link = urlMaker(item) });
                            break;
                        }
                    case OfferDropDownFilter.None:
                    default:
                        dropDown.Items.Add(new DropDownItem { DisplayName = item.Name, Link = urlMaker(item) });
                        break;
                }

            }

			return dropDown;
		}

		public static CalandarJson GetCalandarEvent(this MemberBooking booking, UrlHelper url, bool showOffer = false)
		{
			if (booking == null)
				return null;

			var color = "";
			if (booking.Expired || booking.Cancelled)
			{
				color = CalandarJson.Grey;
			}
			else if (booking.Refused)
			{
				color = CalandarJson.Red;
			}
			else if (booking.Waiting)
			{
				color = CalandarJson.Orange;
			}
			else if (booking.Unknown)
			{
				color = CalandarJson.Yellow;
			}
            else if (booking.Paid)
            {
                color = CalandarJson.Green;
            }
			var allDay = (booking.ToDate - booking.FromDate).TotalHours >= 23;
			var title = booking.Member.GetFullDisplayName();
			if (showOffer)
			{
				title += " - " + booking.Offer.Name;
			}
			var toRet = new CalandarJson
				{
					id = booking.Id,
					title = title,
					start = string.Format("{0:yyyy-MM-dd HH:mm:ss}", booking.FromDate),
					end = string.Format("{0:yyyy-MM-dd HH:mm:ss}", booking.ToDate),
					url = url.Action(MVC.Backoffice.Localisation.ActionNames.BookingDetail, MVC.Backoffice.Localisation.Name, new { id = booking.Id }),
					allDay = allDay,
                    color = color,
					editable = true
				};
			return toRet;
		}

        public static string GetErrors(this System.Data.Entity.Validation.DbEntityValidationException dbException)
        {
            var toRet = "";
            foreach (var error in dbException.EntityValidationErrors)
            {
                foreach (var item in error.ValidationErrors)
                {
                    toRet += item.ErrorMessage + " ;";
                }
            }

            return toRet;
        }

        public static string GetErrors(this ModelStateDictionary modelState)
        {
            var toRet = "";
            var errors = modelState.Values.Where(x => x.Errors.Count >= 1);
            foreach (var error in errors)
            {
                foreach (var item in error.Errors)
                {
                    toRet += item.ErrorMessage+" ;";
                }
            }

            return toRet;
        }
    }
}