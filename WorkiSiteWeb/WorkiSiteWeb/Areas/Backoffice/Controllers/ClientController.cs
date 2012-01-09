using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using Worki.Memberships;
using Postal;

namespace Worki.Web.Areas.Backoffice.Controllers
{
	public partial class ClientController : BackofficeControllerBase
    {
        ILogger _Logger;
        IMembershipService _MembershipService;

        public ClientController(ILogger logger,
                                IMembershipService membershipService)
        {
            _Logger = logger;
            _MembershipService = membershipService;
        }

        public const int PageSize = 5;

        /// <summary>
        /// Get action method to show clients of the owner
        /// </summary>
        /// <returns>View containing the clients</returns>
        public virtual ActionResult List(int? page)
        {
            var id = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var p = page ?? 1;
            try
            {
                var member = mRepo.Get(id);
                Member.Validate(member);

                var clients = member.MemberClients.Select(mc => mc.Client);
                var model = new PagingList<Member>
                {
                    List = clients.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
                    PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = clients.Count() }
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _Logger.Error("List", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

        /// <summary>
        /// Get action method to add a client
        /// </summary>
        /// <returns>View containing the client data</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult Add()
        {
            return View(new ProfilFormViewModel());
        }

        /// <summary>
        /// Post action method to add a client
        /// </summary>
        /// <returns>Redirect to client list if ok</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateOnlyIncomingValues]
        public virtual ActionResult Add(ProfilFormViewModel formData)
        {
            if (ModelState.IsValid)
            {
                var memberId = WebHelper.GetIdentityId(User.Identity);
                var sendNewAccountMail = false;
                try
                {
                    var clientId = 0;
                    sendNewAccountMail = _MembershipService.TryCreateAccount(formData.Member.Email, formData.Member.MemberMainData, out clientId);

                    var context = ModelFactory.GetUnitOfWork();
                    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                    var member = mRepo.Get(memberId);
                    var client = mRepo.Get(clientId);
                    if (member.HasClient(clientId))
                    {
                        throw new Exception("Vous avez déjà ce client");
                    }
                    try
                    {
                        member.MemberClients.Add(new MemberClient { ClientId = clientId });

                        dynamic newMemberMail = null;
                        if (sendNewAccountMail)
                        {
                            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                            var editprofilUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit());
                            TagBuilder profilLink = new TagBuilder("a");
                            profilLink.MergeAttribute("href", editprofilUrl);
                            profilLink.InnerHtml = Worki.Resources.Views.Account.AccountString.EditMyProfile;

                            var editpasswordUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit());
                            TagBuilder passwordLink = new TagBuilder("a");
                            passwordLink.MergeAttribute("href", editpasswordUrl);
                            passwordLink.InnerHtml = Worki.Resources.Views.Account.AccountString.ChangeMyPassword;

                            newMemberMail = new Email(MVC.Emails.Views.Email);
                            newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                            newMemberMail.To = client.Email;
                            newMemberMail.ToName = client.MemberMainData.FirstName;

                            newMemberMail.Subject = Worki.Resources.Email.BookingString.BookingNewMemberSubject;
                            newMemberMail.Content = "compte crée";
                        }

                        context.Commit();

                        if (sendNewAccountMail)
                        {
                            newMemberMail.Send();
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }

                    TempData[MiscHelpers.TempDataConstants.Info] = "Le client a bien été ajouté.";
                    return RedirectToAction(MVC.Backoffice.Client.List());
                }
                catch (Exception ex)
                {
                    _Logger.Error("Add", ex);
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(formData);
        }

    }
}
