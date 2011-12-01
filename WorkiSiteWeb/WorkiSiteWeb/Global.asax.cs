using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Logging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using MvcSiteMapProvider.Web;
using Ninject;
using Ninject.Modules;
using Postal;
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
using System.Configuration;

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

			Bind<IVisitorRepository>()
				.To<VisitorRepository>();

			Bind<IMemberRepository>()
				.To<MemberRepository>();

			Bind<IGroupRepository>()
				.To<GroupRepository>();

			Bind<IEmailService>()
                .To<Postal.EmailService>();

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

            Bind<IPaymentHandler>()
                .To<MemberBookingPaymentHandler>().WithMetadata(PaymentHandlerFactory.Constants.HandlerTypeString, PaymentHandlerFactory.HandlerType.Booking);

			Bind<IPaymentHandler>()
				.To<MemberQuotationPaymentHandler>().WithMetadata(PaymentHandlerFactory.Constants.HandlerTypeString, PaymentHandlerFactory.HandlerType.Quotation);

			Bind<ILogger>().
				To<Log4NetLogger>()
				.InSingletonScope();
			ILogger logger = Kernel.Get<ILogger>();

			logger.Info("Application Started");
		}
	}

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            //It's important to check whether session object is ready
            if (HttpContext.Current.Session != null)
            {
                CultureInfo ci = (CultureInfo)this.Session["Culture"];
                //Checking first if there is no value in session
                //and set default language
                //this can happen for first user's request
                if (ci == null)
                {
                    //Sets default culture to french invariant
                    string langName = "fr";
                    //Try to get values from Accept lang HTTP header
                    //if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length != 0)
                    //{
                    //    //Gets accepted list
                    //    langName = HttpContext.Current.Request.UserLanguages[0].Substring(0, 2);
                    //}
                    ci = new CultureInfo(langName);
                    this.Session["Culture"] = ci;
                }
                //Finally setting culture for each request
                Thread.CurrentThread.CurrentUICulture = ci;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
                Thread.CurrentThread.CurrentCulture.NumberFormat = CultureInfo.InvariantCulture.NumberFormat;
            }
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			#region Home

			//home index
			routes.MapRoute(
				"",
				"",
				new { controller = "Home", action = "Index" },
				new string[] { "Worki.Web.Controllers" }
			);

			routes.MapRoute(
				"",
				"accueil/{action}/",
				new { controller = "Home", action = "Default" },
				new string[] { "Worki.Web.Controllers" }
			);

			#endregion

			#region Profil

			routes.MapRoute(
				"",
				"profil/{action}/{id}",
				new { controller = "Profil", action = "Default", id = UrlParameter.Optional },
				new string[] { "Worki.Web.Controllers" }
			);

			#endregion

			#region Account

			routes.MapRoute(
				"",
				"compte/{action}/{id}",
				new { controller = "Account", action = "Default", id = UrlParameter.Optional },
				new string[] { "Worki.Web.Controllers" }
			);

			#endregion

			#region Localisation

			var localisationTypes = (from t in Localisation.GetLocalisationTypes().Values select MiscHelpers.GetSeoString(t));

			routes.MapRoute(
				"", // Nom d'itinéraire
				"{type}/{id}/{name}", // URL avec des paramètres
				new { area = "", controller = "Localisation", action = "Details" }, // Paramètres par défaut
				new { id = @"\d+", type = new FromValuesListConstraint(localisationTypes) },
				new string[] { "Worki.Web.Controllers" }
			);

			routes.MapRoute(
				"",
				"lieu-de-travail/{action}/{lieu}/{page}/{index}/{offer-type}/{tout}/{order}/",
				new { controller = "Localisation", action = "Default" },
				new string[] { "Worki.Web.Controllers" }
			);

			routes.MapRoute(
				"",
				"lieu-de-travail/{action}/{lieu}/{page}/{offer-type}/{tout}/{order}/",
				new { controller = "Localisation", action = "Default" },
				new string[] { "Worki.Web.Controllers" }
			);

			//old urls
			routes.MapRoute(
				"",
				"recherche/{action}/{lieu}/{page}/{index}/{offer-type}/{tout}/",
				new { controller = "Localisation", action = "Default" },
				new string[] { "Worki.Web.Controllers" }
			);

			routes.MapRoute(
				"",
				"recherche/{action}/{lieu}/{page}/{offer-type}/{tout}/",
				new { controller = "Localisation", action = "Default" },
				new string[] { "Worki.Web.Controllers" }
			);

			//routes.MapRoute(
			//    "",
			//    "lieu-de-travail/{action}/{type}/",
			//    new { controller = "Localisation", action = "Default" },
			//    new string[] { "Worki.Web.Controllers" }
			//);

			routes.MapRoute(
				"", // Nom d'itinéraire
				"lieu-de-travail/details/{id}/{name}", // URL avec des paramètres
				new { area = "", controller = "Localisation", action = "Details" }, // Paramètres par défaut
				new string[] { "Worki.Web.Controllers" }
			);

			routes.MapRoute(
				"", // Nom d'itinéraire
				"lieu-de-travail/{action}/{id}/{type}", // URL avec des paramètres
				new { controller = "Localisation", action = "Offers" }, // Paramètres par défaut
				new string[] { "Worki.Web.Controllers" }
			);

			routes.MapRoute(
				"", // Nom d'itinéraire
				"lieu-de-travail/{action}/{id}", // URL avec des paramètres
				new { controller = "Localisation", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
				new string[] { "Worki.Web.Controllers" }
			);

			routes.MapRoute(
				"", // Nom d'itinéraire
				"recherche/{action}/{id}", // URL avec des paramètres
				new { controller = "Localisation", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
				new string[] { "Worki.Web.Controllers" }
			);

			#endregion

			#region Rental

			routes.MapRoute(
				"", // Nom d'itinéraire
				"annonces/{action}/{id}", // URL avec des paramètres
				new { controller = "Rental", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
				new string[] { "Worki.Web.Controllers" }
			);

			#endregion

			#region Booking

			routes.MapRoute(
				"", // Nom d'itinéraire
				"reservation/{action}/{id}/{localisationId}", // URL avec des paramètres
				new { controller = "Booking", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
				new string[] { "Worki.Web.Controllers" }
			);

			#endregion

			#region Offer

			routes.MapRoute(
				"", // Nom d'itinéraire
				"offre/{action}/{id}/{localisationId}", // URL avec des paramètres
				new { controller = "Offer", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
				new string[] { "Worki.Web.Controllers" }
			);

			#endregion

            // MQP 
            routes.MapRoute(
                "",
                "paywithpaypal/{memberBookingId}",
                new { controller = "Payment", action = "PayWithPayPal" },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.MapRoute(
                "",
                "paypalaccepted/{memberBookingId}",
                new { controller = "Payment", action = "PayPalAccepted" },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.MapRoute(
                "",
                "paypalcancelled/{memberBookingId}",
                new { controller = "Payment", action = "PayPalCancelled" },
                new string[] { "Worki.Web.Controllers" }
            );

            routes.MapRoute(
                "",
                "paypalnotification",
                new { controller = "Payment", action = "PayPalInstantNotification" },
                new string[] { "Worki.Web.Controllers" }
            );


			routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
				new string[] { "Worki.Web.Controllers" }
            );

			//routing error handling
			routes.MapRoute(
				"Error",
				"{*url}",
				new { controller = "Home", action = "Error" },
				new string[] { "Worki.Web.Controllers" }
			);

            //foreach (Route r in routes)
            //{
            //    if (!(r.RouteHandler is SingleCultureMvcRouteHandler))
            //    {
            //        r.RouteHandler = new Worki.Infrastructure.MultiCultureMvcRouteHandler();
            //        r.Url = "{culture}/" + r.Url;
            //        Adding default culture 
            //        if (r.Defaults == null)
            //        {
            //            r.Defaults = new RouteValueDictionary();
            //        }
            //        r.Defaults.Add("culture", Worki.Infrastructure.Culture.fr.ToString());

            //        Adding constraint for culture param
            //        if (r.Constraints == null)
            //        {
            //            r.Constraints = new RouteValueDictionary();
            //        }
            //        r.Constraints.Add("culture", new Worki.Infrastructure.CultureConstraint(Worki.Infrastructure.Culture.fr.ToString(),

            //        Worki.Infrastructure.Culture.fr.ToString()));
            //    }
            //}


        }

		private IKernel _kernel = new StandardKernel(new WorkiInjectModule());

        protected void Application_Start()
        {
			// Inject account repository into our custom membership & role providers.
			_kernel.Inject(Membership.Provider);
			_kernel.Inject(Roles.Provider);

			LocalisationDynamicNodeProvider.RegisterKernel(_kernel);
			ModelFactory.RegisterKernel(_kernel);
            PaymentHandlerFactory.RegisterKernel(_kernel);

			InitialiseAdmin();

			//Inject
			ControllerBuilder.Current.SetControllerFactory(new NinjectControlerFactory(_kernel));

			//routes
            AreaRegistration.RegisterAllAreas();
			XmlSiteMapController.RegisterRoutes(RouteTable.Routes);
            GlobalFilters.Filters.Add(new RedirectMobileDevicesToMobileAreaAttribute(), 1);
            RegisterRoutes(RouteTable.Routes);

			//model binder
            var defaultBinder = new DefaultModelBinder();
            ModelBinders.Binders.DefaultBinder = defaultBinder;
            ModelBinders.Binders.Add(typeof(Localisation), new LocalisationBinder(defaultBinder));
            ModelBinders.Binders.Add(typeof(Rental), new RentalBinder(defaultBinder));
			ModelBinders.Binders.Add(typeof(Offer), new OfferBinder(defaultBinder));			

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
            catch (Exception)
            {
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