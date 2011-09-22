﻿using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Logging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.Modules;
using Worki.Data.Models;
using Worki.Data.Repository;
using Worki.Infrastructure.Email;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Memberships;
using Worki.Web.ModelBinder;
using Worki.Service;
using Worki.Services;
using MvcSiteMapProvider.Web;
using Worki.SiteMap;
using Worki.Rest.Routing;
using System.Web;
using System.Globalization;
using System.Threading;
using System;

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
				.To<EmailService>();

			Bind<IWelcomePeopleRepository>()
				.To<WelcomePeopleRepository>();

            Bind<IPressRepository>()
                .To<PressRepository>();

			Bind<ISearchService>()
				.To<SearchService>();

			Bind<IBookingRepository>()
				.To<BookingRepository>();

			Bind<IRentalRepository>()
                .To<RentalRepository>();

			Bind<IGeocodeService>()
				.To<GeocodeService>();

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
                    if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length != 0)
                    {
                        //Gets accepted list
                        langName = HttpContext.Current.Request.UserLanguages[0].Substring(0, 2);
                    }
                    ci = new CultureInfo(langName);
                    this.Session["Culture"] = ci;
                }
                //Finally setting culture for each request
                Thread.CurrentThread.CurrentUICulture = ci;
                //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
            }
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

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

            routes.MapRoute(
				"",
				"recherche/{action}/{lieu}/{page}/{index}/{offer-type}/{tout}/",
				new { controller = "Search", action = "Default" },
				new string[] { "Worki.Web.Controllers" }
            );

            routes.MapRoute(
				"",
				"recherche/{action}/{lieu}/{page}/{offer-type}/{tout}/",
				new { controller = "Search", action = "Default" },
				new string[] { "Worki.Web.Controllers" }
            );

            routes.MapRoute(
				"",
				"recherche/{action}/{type}/",
				new { controller = "Search", action = "Default" },
				new string[] { "Worki.Web.Controllers" }
            );

            routes.MapRoute(
				"",
				"recherche/{action}/",
				new { controller = "Search", action = "Default" },
				new string[] { "Worki.Web.Controllers" }
            );

			routes.MapRoute(
				"",
				"profil/{action}/{id}",
				new { controller = "Profil", action = "Default", id = UrlParameter.Optional },
				new string[] { "Worki.Web.Controllers" }
			);

            routes.MapRoute(
				"",
				"compte/{action}/{id}",
				new { controller = "Account", action = "Default", id = UrlParameter.Optional },
				new string[] { "Worki.Web.Controllers" }
            );

            routes.MapRoute(
                "", // Nom d'itinéraire
                "lieu-de-travail/details/{id}/{name}", // URL avec des paramètres
                new { area="", controller = "Localisation", action = "Details", id = 0, name = "" }, // Paramètres par défaut
				new string[] { "Worki.Web.Controllers" }
            );

            routes.MapRoute(
                "", // Nom d'itinéraire
                "lieu-de-travail/{action}/{id}", // URL avec des paramètres
				new { controller = "Localisation", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
				new string[] { "Worki.Web.Controllers" }
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
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


			// Register REST api routes within path /api
			RestRoutes.RegisterRoutes(routes, "/api");
        }

		private IKernel _kernel = new StandardKernel(new WorkiInjectModule());

        protected void Application_Start()
        {
			// Inject account repository into our custom membership & role providers.
			_kernel.Inject(Membership.Provider);
			_kernel.Inject(Roles.Provider);

			LocalisationDynamicNodeProvider.RegisterKernel(_kernel);

			//Inject
			ControllerBuilder.Current.SetControllerFactory(new NinjectControlerFactory(_kernel));

			//routes
            AreaRegistration.RegisterAllAreas();
			XmlSiteMapController.RegisterRoutes(RouteTable.Routes);
            RegisterRoutes(RouteTable.Routes);

			//model binder
            var defaultBinder = new DefaultModelBinder();
            ModelBinders.Binders.DefaultBinder = defaultBinder;
            ModelBinders.Binders.Add(typeof(Localisation), new LocalisationBinder(defaultBinder));
            ModelBinders.Binders.Add(typeof(Rental), new RentalBinder(defaultBinder));

			//resources
			DefaultModelBinder.ResourceClassKey = "Messages";
        }

		#region Privates

		private static object _gate = new object();
		private static bool _initialized = false;

		#endregion

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
    }
}