using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Data.Repository;
using Worki.Data.Models;
using System.Linq;
using Worki.Infrastructure;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure.Helpers;
using System.Web.Security;
using Worki.Memberships;

namespace Worki.Web.Areas.Backoffice.Controllers
{
	public partial class ProfilController : BackofficeControllerBase
	{
		#region Private

		IMembershipService _MembershipService;

		#endregion

		public ProfilController(ILogger logger,
                                IObjectStore objectStore,
                                IMembershipService membershipService)
            : base(logger, objectStore)
		{
			_MembershipService = membershipService;
		}

        /// <summary>
        /// GET Action method to change payment information
        /// </summary>
        /// <returns>the form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult ChangePaymentInformation()
        {
            var id = WebHelper.GetIdentityId(User.Identity);
            if (id == 0)
                return View(MVC.Shared.Views.Error);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var member = mRepo.Get(id);

            return View(MVC.Backoffice.Profil.Views.ChangePaymentInformation, new PaymentInfoModel(member));
        }

        /// <summary>
        /// POST Action method to change payment information
        /// </summary>
        /// <param name="model">The change payment information data from the form</param>
        /// <returns>Back office home page if ok, the form with error if not</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ChangePaymentInformation(PaymentInfoModel model)
        {
            var id = WebHelper.GetIdentityId(User.Identity);
            if (id == 0)
                return View(MVC.Shared.Views.Error);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var member = mRepo.Get(id);

            if (member == null)
                return View(MVC.Shared.Views.Error);

            if (ModelState.IsValid)
            {
                if (_MembershipService.ValidateUser(member.Username, model.WorkiPassword))
                {
                    try
                    {
                        model.ChangePaymentInformation(member);
                        context.Commit();

                        TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.PaymentInfoModified;
                        return RedirectToAction(MVC.Backoffice.Home.Index());
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error("ChangePaymentInformation", ex);
                        context.Complete();
                    }
                }
                else
                {
                    ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.MailOrPasswordNotCorrect);
                }
            }

            return View(model);
        }

		/// <summary>
		/// GET Action method to change billing information
		/// </summary>
		/// <returns>the form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult ChangeBillingInformation()
		{
			var id = WebHelper.GetIdentityId(User.Identity);
			if (id == 0)
				return View(MVC.Shared.Views.Error);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var member = mRepo.Get(id);

			return View(member);
		}

		/// <summary>
		/// POST Action method to change billing information
		/// </summary>
		/// <param name="model">The change billing information data from the form</param>
		/// <returns>Back office home page if ok, the form with error if not</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[ValidateOnlyIncomingValues]
		[ValidateAntiForgeryToken]
		public virtual ActionResult ChangeBillingInformation(Member model)
		{
			var id = WebHelper.GetIdentityId(User.Identity);
			if (id == 0)
				return View(MVC.Shared.Views.Error);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var member = mRepo.Get(id);

			if (member == null)
				return View(MVC.Shared.Views.Error);

			if (ModelState.IsValid)
			{
				try
				{
					TryUpdateModel(member);
					context.Commit();

					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.PaymentInfoModified;
					return RedirectToAction(MVC.Backoffice.Home.Index());
				}
				catch (Exception ex)
				{
					_Logger.Error("ChangeBillingInformation", ex);
					context.Complete();
				}
			}

			return View(model);
		}
	}
}
