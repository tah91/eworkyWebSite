using System.Web.Mvc;
using Worki.Data.Models;
using System;
using Worki.Web.Helpers;

namespace Worki.Web.ModelBinder
{
	public class LocalisationCartBinder : IModelBinder
	{
		private readonly IModelBinder _Binder;

        public LocalisationCartBinder(IModelBinder binder)
		{
			_Binder = binder;
		}

        const string _CartSessionKey = "cartSessionKey";

		/// <summary>
		/// custom binding of localisation model : extract from session
		/// </summary>
		/// <param name="controllerContext">contains the cart data to fill model</param>
		/// <param name="bindingContext">to add modelerrors if needed</param>
		/// <returns></returns>
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
            var controller = controllerContext.Controller as Controller;
            var isLoggued = WebHelper.GetIdentityId(controller.User.Identity) != 0;
            LocalisationCart sessionCart = controllerContext.HttpContext.Session[_CartSessionKey] as LocalisationCart;
            if (sessionCart == null)
            {
                sessionCart = new LocalisationCart();
                controllerContext.HttpContext.Session[_CartSessionKey] = sessionCart;
            }

            sessionCart.NeedNewAccount = !isLoggued;

            LocalisationCart formCart = _Binder.BindModel(controllerContext, bindingContext) as LocalisationCart;
            if (formCart != null)
            {
                sessionCart.CopyData(formCart);
            }

			return sessionCart;
		}
	}

}