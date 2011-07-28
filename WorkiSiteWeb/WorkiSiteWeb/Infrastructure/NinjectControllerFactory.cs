using System;
using System.Web.Mvc;
using Ninject;
using Ninject.Modules;
using WorkiSiteWeb.Infrastructure.Repository;
using WorkiSiteWeb.Models;
using WorkiSiteWeb.Infrastructure.Membership;
using WorkiSiteWeb.Infrastructure.Logging;
using Logging;
using WorkiSiteWeb.Infrastructure.Email;
using System.Web.Security;

public class NinjectControlerFactory : DefaultControllerFactory
{
	private IKernel _Kernel;

	public NinjectControlerFactory(IKernel kernel)
    {
		_Kernel = kernel;
    }

    protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
    {
        if (controllerType == null)
            return null;

		return (IController)_Kernel.Get(controllerType);
    }
}