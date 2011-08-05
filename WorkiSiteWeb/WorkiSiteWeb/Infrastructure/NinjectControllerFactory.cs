using System;
using System.Web.Mvc;
using Ninject;

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