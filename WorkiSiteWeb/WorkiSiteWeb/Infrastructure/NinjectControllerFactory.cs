using System;
using System.Web.Mvc;
using Ninject;
using Worki.Infrastructure;
using System.Web;
using System.Collections.Generic;

namespace Worki.Web
{
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

    public class SessionStore : IObjectStore
    {
        public void Delete(string key)
        {
            HttpContext.Current.Session.Remove(key);
        }

        public T Get<T>(string key)
        {
            return (T)HttpContext.Current.Session[key];
        }

        public void Store<T>(string key, T value)
        {
            HttpContext.Current.Session[key] = value;
        }

        public IList<T> GetList<T>(string key)
        {
            throw new NotImplementedException();
        }
    }
}