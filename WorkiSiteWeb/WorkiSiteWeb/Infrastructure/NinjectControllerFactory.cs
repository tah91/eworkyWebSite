﻿using System;
using System.Web.Mvc;
using Ninject;
using Ninject.Modules;
using Worki.Web.Infrastructure.Repository;
using Worki.Web.Models;
using Worki.Web.Infrastructure.Membership;
using Worki.Web.Infrastructure.Logging;
using Logging;
using Worki.Web.Infrastructure.Email;
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