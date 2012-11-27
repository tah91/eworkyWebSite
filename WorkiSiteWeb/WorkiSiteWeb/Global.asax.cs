using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Logging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using MvcSiteMapProvider.Web;
using Ninject;
using Ninject.Modules;
using Worki.Data.Models;
using Worki.Infrastructure;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure.UnitOfWork;
using Worki.Memberships;
using Worki.Service;
using Worki.SiteMap;
using Worki.Web.Helpers;
using Worki.Web.ModelBinder;
using Worki.Infrastructure.Email;
using System.Globalization;
using System.Collections.Generic;

namespace Worki.Web
{
    // Remarque : pour obtenir des instructions sur l'activation du mode classique IIS6 ou IIS7, 
    // visitez http://go.microsoft.com/?LinkId=9394801

	class WorkiInjectModule : NinjectModule
	{
		public override void Load()
		{
			Bind<ILocalisationRepository>()
				.To<LocalisationRepository>();


			Bind<IFormsAuthenticationService>()
				.To<FormsAuthenticationService>();

			Bind<IMembershipService>()
				.To<AccountMembershipService>();

			Bind<IMemberRepository>()
				.To<MemberRepository>();

			Bind<IGroupRepository>()
				.To<GroupRepository>();

            if (WebHelper.IsDebug())
            {
                Bind<IEmailService>()
                    .To<EmailService>();
            }
            else
            {
                Bind<IEmailService>()
                    .To<SendGridEmailService>();
            }

			Bind<IWelcomePeopleRepository>()
				.To<WelcomePeopleRepository>();

            Bind<IPressRepository>()
                .To<PressRepository>();

			Bind<ISearchService>()
				.To<SearchService>();

			Bind<IBookingRepository>()
				.To<BookingRepository>();

			Bind<IQuotationRepository>()
				.To<QuotationRepository>();

			Bind<IRentalRepository>()
                .To<RentalRepository>();

            Bind<IRentalSearchService>()
                .To<RentalSearchService>();

			Bind<IGeocodeService>()
				.To<GeocodeService>();

			Bind<IBlogService>()
				.To<BlogService>();

			Bind<IUnitOfWork>()
				.To<WorkiDBEntities>();

			Bind<IOfferRepository>()
				.To<OfferRepository>();

            Bind<IPaymentService>()
                .To<PaymentService>();

            Bind<ITransactionRepository>()
                .To<TransactionRepository>();

			Bind<IQuotationTransactionRepository>()
				.To<QuotationTransactionRepository>();

            Bind<IBookingLogRepository>()
               .To<BookingLogRepository>();

			Bind<IQuotationLogRepository>()
				.To<QuotationLogRepository>();

            Bind<IObjectStore>()
                .To<SessionStore>();

            Bind<IPaymentHandler>()
                .To<MemberBookingPaymentHandler>().WithMetadata(PaymentHandlerFactory.Constants.HandlerTypeString, PaymentHandlerFactory.HandlerType.Booking);

			Bind<IPaymentHandler>()
				.To<MemberQuotationPaymentHandler>().WithMetadata(PaymentHandlerFactory.Constants.HandlerTypeString, PaymentHandlerFactory.HandlerType.Quotation);

            Bind<IInvoiceRepository>()
                .To<InvoiceRepository>();

			Bind<IInvoiceService>()
				.To<InvoiceService>();

			Bind<INonceRepository>()
				.To<DatabaseKeyNonceStore>();

			Bind<IApiClientRepository>()
				.To<ApiClientRepository>();

			Bind<IApiClientAuthorizationRepository>()
				.To<ApiClientAuthorizationRepository>();

			Bind<ILogger>().
				To<Log4NetLogger>()
				.InSingletonScope();
		}
	}

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            #region Home

            CultureInfo cultureFR = CultureInfo.GetCultureInfo("fr");

            DictionaryRouteValueTranslationProvider homeActions = new DictionaryRouteValueTranslationProvider(
                new List<RouteValueTranslation> {
                    new RouteValueTranslation(cultureFR, "add-space", "ajouter-espace"),
                    new RouteValueTranslation(cultureFR, "how-it-works", "comment-ca-marche"),
                    new RouteValueTranslation(cultureFR, "user-notice", "mode-d'emploi-utilisateur"),
                    new RouteValueTranslation(cultureFR, "owner-notice", "mode-d'emploi-gerant"),
                    new RouteValueTranslation(cultureFR, "pricing", "espace-gerant"),
                    new RouteValueTranslation(cultureFR, "share-office", "partager-un-bureau"),
                    new RouteValueTranslation(cultureFR, "about", "a-propos"),
                    new RouteValueTranslation(cultureFR, "team", "l'equipe"),
                    new RouteValueTranslation(cultureFR, "press", "presse")
                }
            );

            //home index
			routes.CultureMapRoute(
                "",
                "",
                new { controller = "Home", action = "Index" },
                null,
                new string[] { "Worki.Web.Controllers" },
                new { action = homeActions }
            );

            routes.SpecificCultureMapRoute(
                "",
                "accueil/{action}/",
                new { controller = "Home", action = "Index" },
                null,
                new string[] { "Worki.Web.Controllers" },
                "fr",
                new { action = homeActions }
            );

            routes.CultureMapRoute(
                "",
                "home/{action}/",
                new { controller = "Home", action = "Index" },
                null,
                new string[] { "Worki.Web.Controllers" },
                new { action = homeActions }
            );

            #endregion

            #region Profil

            routes.CultureMapRoute(
                "",
                "profil/{action}/{id}",
                new { controller = "Profil", action = "Default", id = UrlParameter.Optional },
                new string[] { "Worki.Web.Controllers" }
            );

            #endregion

            #region Account

            routes.CultureMapRoute(
                "",
                "account/{action}/{id}",
                new { controller = "Account", action = "Default", id = UrlParameter.Optional },
                new string[] { "Worki.Web.Controllers" }
            );

            #region Old Routes

            routes.CultureMapRoute(
                "",
                "compte/connexion/{id}",
                new { controller = "Account", action = "LogOn", id = UrlParameter.Optional },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.CultureMapRoute(
                "",
                "compte/deconnexion/{id}",
                new { controller = "Account", action = "LogOff", id = UrlParameter.Optional },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.CultureMapRoute(
                "",
                "compte/inscription/{id}",
                new { controller = "Account", action = "Register", id = UrlParameter.Optional },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.CultureMapRoute(
                "",
                "compte/activer/{id}",
                new { controller = "Account", action = "Activate", id = UrlParameter.Optional },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.CultureMapRoute(
                "",
                "compte/reset-mdp/{id}",
                new { controller = "Account", action = "ResetPassword", id = UrlParameter.Optional },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.CultureMapRoute(
                "",
                "compte/{action}/{id}",
                new { controller = "Account", action = "Default", id = UrlParameter.Optional },
                new string[] { "Worki.Web.Controllers" }
            );

            #endregion
            

            #endregion

            #region Localisation

            DictionaryRouteValueTranslationProvider localisationTypes = new DictionaryRouteValueTranslationProvider(
                new List<RouteValueTranslation> {
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.SpotWifi, MiscHelpers.SeoConstantsFr.SpotWifi),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.CoffeeResto, MiscHelpers.SeoConstantsFr.CoffeeResto),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.Biblio, MiscHelpers.SeoConstantsFr.Biblio),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.PublicSpace, MiscHelpers.SeoConstantsFr.PublicSpace),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.TravelerSpace, MiscHelpers.SeoConstantsFr.TravelerSpace),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.Hotel, MiscHelpers.SeoConstantsFr.Hotel),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.Telecentre, MiscHelpers.SeoConstantsFr.Telecentre),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.BuisnessCenter, MiscHelpers.SeoConstantsFr.BuisnessCenter),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.CoworkingSpace, MiscHelpers.SeoConstantsFr.CoworkingSpace),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.WorkingHotel, MiscHelpers.SeoConstantsFr.WorkingHotel),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.PrivateArea, MiscHelpers.SeoConstantsFr.PrivateArea),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.SharedOffice, MiscHelpers.SeoConstantsFr.SharedOffice)
                }
            );

            routes.SpecificCultureMapRoute(
                "", // Nom d'itinéraire
                "{type}/{id}/{name}", // URL avec des paramètres
                new { area = "", controller = "Localisation", action = "Details" }, // Paramètres par défaut
                new { id = @"\d+", type = new FromValuesListConstraint(MiscHelpers.SeoConstants.AllLocalisationTypes) },
                new string[] { "Worki.Web.Controllers" },
                "fr",
                new { type = localisationTypes }
            );

            routes.CultureMapRoute(
                "", // Nom d'itinéraire
                "{type}/{id}/{name}", // URL avec des paramètres
                new { area = "", controller = "Localisation", action = "Details" }, // Paramètres par défaut
                new { id = @"\d+", type = new FromValuesListConstraint(MiscHelpers.SeoConstants.AllLocalisationTypes) },
                new string[] { "Worki.Web.Controllers" },
                new { type = localisationTypes }
            );

            routes.SpecificCultureMapRoute(
                "", // Nom d'itinéraire
                "{type}/{place}", // URL avec des paramètres
                new { controller = "Localisation", action = "FullSearchByTypeSeo" }, // Paramètres par défaut
                new { type = new FromValuesListConstraint(MiscHelpers.SeoConstants.AllLocalisationTypes) },
                new string[] { "Worki.Web.Controllers" },
                "fr",
                new { type = localisationTypes }
            );

            routes.CultureMapRoute(
                "", // Nom d'itinéraire
                "{type}/{place}", // URL avec des paramètres
                new { controller = "Localisation", action = "FullSearchByTypeSeo" }, // Paramètres par défaut
                new { type = new FromValuesListConstraint(MiscHelpers.SeoConstants.AllLocalisationTypes) },
                new string[] { "Worki.Web.Controllers" },
                new { type = localisationTypes }
            );

            DictionaryRouteValueTranslationProvider offerTypes = new DictionaryRouteValueTranslationProvider(
                new List<RouteValueTranslation> {
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.FreeArea, MiscHelpers.SeoConstantsFr.FreeArea),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.BuisnessLounge, MiscHelpers.SeoConstantsFr.BuisnessLounge),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.Workstation, MiscHelpers.SeoConstantsFr.Workstation),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.Desktop, MiscHelpers.SeoConstantsFr.Desktop),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.MeetingRoom, MiscHelpers.SeoConstantsFr.MeetingRoom),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.SeminarRoom, MiscHelpers.SeoConstantsFr.SeminarRoom),
                    new RouteValueTranslation(cultureFR, MiscHelpers.SeoConstants.VisioRoom, MiscHelpers.SeoConstantsFr.VisioRoom)
                }
            );

            routes.SpecificCultureMapRoute(
                "", // Nom d'itinéraire
                "{offerType}/{place}", // URL avec des paramètres
                new { controller = "Localisation", action = "FullSearchByOfferSeo" }, // Paramètres par défaut
                new { offerType = new FromValuesListConstraint(MiscHelpers.SeoConstants.AllOfferTypes) },
                new string[] { "Worki.Web.Controllers" },
                "fr",
                new { offerType = offerTypes }
            );

            routes.CultureMapRoute(
                "", // Nom d'itinéraire
                "{offerType}/{place}", // URL avec des paramètres
                new { controller = "Localisation", action = "FullSearchByOfferSeo" }, // Paramètres par défaut
                new { offerType = new FromValuesListConstraint(MiscHelpers.SeoConstants.AllOfferTypes) },
                new string[] { "Worki.Web.Controllers" },
                new { offerType = offerTypes }
            );

            routes.CultureMapRoute(
                "",
				"workspaces/{action}/{place}/{lat}/{lng}/{type}/{offerType}/{page}/{order}/{index}",
                new { controller = "Localisation", action = "FullSearch" },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.CultureMapRoute(
                "", // Nom d'itinéraire
				"workspaces/{action}/{id}", // URL avec des paramètres
                new { controller = "Localisation", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
                new string[] { "Worki.Web.Controllers" }
            );

            #endregion

            #region Rental

            routes.CultureMapRoute(
                "", // Nom d'itinéraire
                "rental/{action}/{id}", // URL avec des paramètres
                new { controller = "Rental", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
                new string[] { "Worki.Web.Controllers" }
            );

            #endregion

            #region Booking

            routes.CultureMapRoute(
                "", // Nom d'itinéraire
                "booking/{action}/{id}/{localisationId}", // URL avec des paramètres
                new { controller = "Booking", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
                new string[] { "Worki.Web.Controllers" }
            );

            #endregion

            #region Offer

            routes.CultureMapRoute(
                "", // Nom d'itinéraire
                "offer/{action}/{id}/{localisationId}", // URL avec des paramètres
                new { controller = "Offer", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
                new string[] { "Worki.Web.Controllers" }
            );

            #endregion

            #region Paypal

            routes.CultureMapRoute(
                "",
                "paywithpaypal/{memberBookingId}",
                new { controller = "Payment", action = "PayWithPayPal" },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.CultureMapRoute(
                "",
                "paypalaccepted/{memberBookingId}",
                new { controller = "Payment", action = "PayPalAccepted" },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.CultureMapRoute(
                "",
                "paypalcancelled/{memberBookingId}",
                new { controller = "Payment", action = "PayPalCancelled" },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.CultureMapRoute(
                "",
                "paypalnotification",
                new { controller = "Payment", action = "PayPalInstantNotification" },
                new string[] { "Worki.Web.Controllers" }
            );

            #endregion

            routes.CultureMapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters,
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
                new string[] { "Worki.Web.Controllers" }
            );

            //routing error handling
            routes.CultureMapRoute(
                "Error",
                "{*url}",
                new { controller = "Home", action = "Error" },
                new string[] { "Worki.Web.Controllers" }
            );
        }

		private IKernel _Kernel = new StandardKernel(new WorkiInjectModule());

        private ILogger _Logger;

        protected void Application_Start()
        {
            _Logger = _Kernel.Get<ILogger>();

            _Logger.Info("Application Started");

            // Inject account repository into our custom membership & role providers.
			_Kernel.Inject(Membership.Provider);
			_Kernel.Inject(Roles.Provider);

			LocalisationDynamicNodeProvider.RegisterKernel(_Kernel);
			ModelFactory.RegisterKernel(_Kernel);
            PaymentHandlerFactory.RegisterKernel(_Kernel);

			InitialiseAdmin();

			//Inject
			ControllerBuilder.Current.SetControllerFactory(new NinjectControlerFactory(_Kernel));

			//routes
            AreaRegistration.RegisterAllAreas();
			XmlSiteMapController.RegisterRoutes(RouteTable.Routes);
            GlobalFilters.Filters.Add(new RedirectMobileDevicesToMobileAreaAttribute(), 1);
            RegisterRoutes(RouteTable.Routes);
			//RouteDebug.RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);

			//model binder
            var defaultBinder = new DefaultModelBinder();
            ModelBinders.Binders.DefaultBinder = defaultBinder;
            ModelBinders.Binders.Add(typeof(Localisation), new LocalisationBinder(defaultBinder));
            ModelBinders.Binders.Add(typeof(Rental), new RentalBinder(defaultBinder));
			ModelBinders.Binders.Add(typeof(Offer), new OfferBinder(defaultBinder));
            ModelBinders.Binders.Add(typeof(LocalisationCart), new LocalisationCartBinder(defaultBinder));

			//resources
			DefaultModelBinder.ResourceClassKey = "Messages";
        }

		#region Initialisation

        static bool _AdminInitialized = false;

        void InitialiseAdmin()
        {
            if (_AdminInitialized)
                return;
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            try
            {

                //create roles
                if (!Roles.RoleExists(MiscHelpers.AdminConstants.AdminRole))
                {
                    Roles.CreateRole(MiscHelpers.AdminConstants.AdminRole);
                }

                //create bo roles
                if (!Roles.RoleExists(MiscHelpers.BackOfficeConstants.BackOfficeRole))
                {
                    Roles.CreateRole(MiscHelpers.BackOfficeConstants.BackOfficeRole);
                }

                //create admin
                var user = mRepo.GetMember(MiscHelpers.AdminConstants.AdminMail);

                //create admin
                if (user == null)
                {
                    MembershipCreateStatus status;
                    Membership.Provider.CreateUser(MiscHelpers.AdminConstants.AdminMail, MiscHelpers.AdminConstants.AdminPass, MiscHelpers.AdminConstants.AdminMail, null, null, true, null, out status);
                }
                //add role
                if (!Roles.IsUserInRole(MiscHelpers.AdminConstants.AdminMail, MiscHelpers.AdminConstants.AdminRole))
                    Roles.AddUserToRole(MiscHelpers.AdminConstants.AdminMail, MiscHelpers.AdminConstants.AdminRole);
                if (!Roles.IsUserInRole(MiscHelpers.AdminConstants.AdminMail, MiscHelpers.BackOfficeConstants.BackOfficeRole))
                    Roles.AddUserToRole(MiscHelpers.AdminConstants.AdminMail, MiscHelpers.BackOfficeConstants.BackOfficeRole);
                //add member data
                if (user.MemberMainData == null)
                {
                    user.MemberMainData = new MemberMainData { FirstName = MiscHelpers.AdminConstants.AdminMail, LastName = MiscHelpers.AdminConstants.AdminMail, Civility = (int)CivilityType.Mr };
                }
                context.Commit();
            }
            catch (Exception ex)
            {
                _Logger.Error("InitialiseAdmin", ex);
                context.Complete();
                return;
            }
			if (WebHelper.IsDebug())
			{
				var email = ConfigurationManager.AppSettings["TestEmail"];
				if (!string.IsNullOrEmpty(email))
				{
					MiscHelpers.EmailConstants.ContactMail = email;
					MiscHelpers.EmailConstants.BookingMail = email;
				}

				MultiCultureMvcRouteHandler.DefaultCulture = Culture.fr;
			}
            _AdminInitialized = true;
        }

		private static object _gate = new object();
		private static bool _initialized = false;

        protected void Application_BeginRequest()
        {
            // Had to move azure role initialization here
            // See http://social.msdn.microsoft.com/Forums/en-US/windowsazuredevelopment/thread/10d042da-50b1-4930-b0c0-aff22e4144f9 
            // and http://social.msdn.microsoft.com/Forums/en-US/windowsazuredevelopment/thread/ab6d56dc-154d-4aba-8bde-2b7f7df121c1/#89264b8c-7e25-455a-8fd6-20f547ab545b

            if (_initialized)
            {
                return;
            }

            lock (_gate)
            {
                if (!_initialized)
                {
                    // Moved all this diagnostics and configuration setup from WebRole.cs
                    // See http://blog.smarx.com/posts/how-to-resolve-setconfigurationsettingpublisher-needs-to-be-called-before-fromconfigurationsetting-can-be-used-after-moving-to-windows-azure-sdk-1-3

                    #region Setup CloudStorageAccount Configuration and Azure Diagnostics
                    if (RoleEnvironment.IsAvailable)
                    {
                        //(CDLTLL) Configuration for Windows Azure settings 
                        CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSettingPublisher) =>
                        {
                            var connectionString = RoleEnvironment.GetConfigurationSettingValue(configName);
                            configSettingPublisher(connectionString);
                        }
                        );

                        AzureAppender.ConfigureAzureDiagnostics();

                    }
                    #endregion

                    _initialized = true;
                }
            }
        }

		#endregion
    }
}