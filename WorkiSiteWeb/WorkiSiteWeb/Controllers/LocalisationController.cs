using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Worki.Data.Models;
using Worki.Infrastructure;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Services;
using Worki.Infrastructure.Helpers;

namespace Worki.Web.Controllers
{
    [HandleError]
    //[ValidateOnlyOnSubmit(ButtonName="valid")]
    public partial class LocalisationController : Controller
    {
        ILocalisationRepository _LocalisationRepository;
        IMemberRepository _MemberRepository;
        ILogger _Logger;
		ISearchService _SearchService;

		public LocalisationController(ILocalisationRepository localisationRepository, IMemberRepository memberRepository, ILogger logger, ISearchService searchService)
		{
			_LocalisationRepository = localisationRepository;
			_MemberRepository = memberRepository;
			_Logger = logger;
			_SearchService = searchService;
		}

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <returns>The action result.</returns>
        public virtual ActionResult Index(int? page)
        {
            var viewModel = new LocalisationListViewModel();
            return View(viewModel);
        }

        /// <summary>
        /// The view containing the details of a localisation
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("details")]
        public virtual ActionResult Details(int id, string name)
        {
            var localisation = _LocalisationRepository.Get(id);
			if (localisation == null || string.IsNullOrEmpty(name))
				return View(MVC.Localisation.Views.lieu_absent);
			else
			{
				var container = new SearchSingleResultViewModel { Localisation = localisation };
				return View(MVC.Shared.Views.resultats_detail, container);
			}
        }

        /// <summary>
        /// GET action to create a new localisation
        /// </summary>
        /// <returns>The form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        [ActionName("ajouter")]
        public virtual ActionResult Create()
        {
            return View(MVC.Localisation.Views.editer, new LocalisationFormViewModel());
        }

        /// <summary>
        /// GET action to adit an existing localisation
        /// </summary>
        /// <returns>The form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        [ActionName("editer")]
        public virtual ActionResult Edit(int? id)
        {
            if (!id.HasValue)
                return View(MVC.Localisation.Views.lieu_absent);
            var localisation = _LocalisationRepository.Get(id.Value);
            if (localisation == null)
                return View(MVC.Localisation.Views.lieu_absent);
            return View(new LocalisationFormViewModel(localisation));
        }

        const string PictureDataString = "PictureData";
        const string _DeleteType = "POST";
		const string LocalisationPrefix = "Localisation";

        /// <summary>
        /// POST action to edit/create a localisation
        /// upload image files or remove files -> do it via js
        /// create the localisation if id has no value, update in database if there is an id
        /// </summary>
        /// <param name="localisation">The localisation data from the form (provided from custom model binder)</param>
        /// <param name="id">The id of the edited localisation</param>
        /// <returns>the detail view of localistion if ok, the form with errors else</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		[ActionName("editer")]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Edit(Localisation localisation, int? id)
		{
			var error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
			//to keep files state in case of error
			TempData[PictureDataString] = new PictureDataContainer(localisation);
			try
			{
				var member = _MemberRepository.GetMember(User.Identity.Name);
				if (!member.IsValidUser())
				{
					error = Worki.Resources.Validation.ValidationString.InvalidUser;
					throw new Exception(error);
				}
				if (ModelState.IsValid)
				{
					var localisationToAdd = new Localisation();
					var idToRedirect = 0;
					var modifType = (!id.HasValue || id.Value == 0) ? EditionType.Creation : EditionType.Edition;
					if (modifType == EditionType.Creation)
					{
						//update
						UpdateModel(localisationToAdd, LocalisationPrefix);
						localisationToAdd.OwnerID = member.MemberId;
						//validate
						_SearchService.ValidateLocalisation(localisationToAdd, ref error);
						//save
						_LocalisationRepository.Add(localisationToAdd);
						idToRedirect = localisationToAdd.ID;
					}
					else
					{
						var editionAccess = member.HasEditionAccess(Roles.IsUserInRole(MiscHelpers.AdminRole));
						if (!string.IsNullOrEmpty(editionAccess))
						{
							error = editionAccess;
							throw new Exception(editionAccess);
						}
						_LocalisationRepository.Update(id.Value, loc => { UpdateModel(loc, LocalisationPrefix); });
						idToRedirect = id.Value;						
					}
					_MemberRepository.Update(member.MemberId, m =>
					{
						m.MemberEditions.Add(new MemberEdition { ModificationDate = DateTime.Now, LocalisationId = idToRedirect, ModificationType = (int)EditionType.Edition });
					});
					TempData.Remove(PictureDataString);
					return RedirectToAction(MVC.Localisation.ActionNames.Details, new { id = idToRedirect, name = ControllerHelpers.GetSeoTitle(localisation.Name) });
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("Edit", ex);
				ModelState.AddModelError("", error);
			}
			return View(new LocalisationFormViewModel(localisation));
		}

		/// <summary>
		/// Action method to handle the json upload of files
		/// </summary>
		/// <returns>json result containing uploaded data</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult UploadFiles()
		{
			var toRet = new List<ImageJson>();
			var urlHelper = new UrlHelper(ControllerContext.RequestContext);
			foreach (string name in Request.Files)
			{
				try
				{
					var postedFile = Request.Files[name];
					if (postedFile == null || string.IsNullOrEmpty(postedFile.FileName))
						continue;
					var uploadedFileName = this.UploadFile(postedFile);
					var url = ControllerHelpers.GetUserImagePath(uploadedFileName);
					var deleteUrl = urlHelper.Action(MVC.Localisation.DeleteImage(uploadedFileName));

					toRet.Add(new ImageJson
					{
						name = uploadedFileName,
						delete_type = _DeleteType,
						delete_url = deleteUrl,
						thumbnail_url = url,
						size = postedFile.ContentLength,
						url = url,
						is_default = "true",
						is_logo = null
					});
				}
				catch (Exception ex)
				{
					_Logger.Error("Edit", ex);
					ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.ErrorWhenSave);
				}
			}
			return Json(toRet, "text/html");
		}

        /// <summary>
        /// GET action to get localisation pictures
        /// in case of data in tempdata, get it
        /// else if id not null, get data from database
        /// else return empty list
        /// return files description in json format
        /// </summary>
        /// <param name="id">The id of the edited localisation</param>
        /// <returns>localisation image desc in json format</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult LoadFiles(int id)
        {
            var toRet = new List<ImageJson>();
            //to recover files state in case of error
            var filesFromCache = TempData[PictureDataString] as PictureDataContainer;
			if (filesFromCache == null)
            {
                if (id == 0)
                    return Json(toRet, "text/html");
                var loc = _LocalisationRepository.Get(id);
                if (loc == null)
                    return Json(toRet, "text/html");
				filesFromCache = new PictureDataContainer(loc);
            }

            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
			foreach (var file in filesFromCache.Files)
            {
				var url = ControllerHelpers.GetUserImagePath(file.FileName);
                var deleteUrl = urlHelper.Action(MVC.Localisation.DeleteImage(file.FileName));
                toRet.Add(new ImageJson
                {
                    name = file.FileName,
                    delete_type = _DeleteType,
                    delete_url = deleteUrl,
                    thumbnail_url = url,
                    url = url,
                    is_default = file.IsDefault ? "true" : null,
                    is_logo = file.IsLogo ? "true" : null
                });
            }
            return Json(toRet, "text/html");
        }

        /// <summary>
        /// Delete action to remove localisation picture
        /// put files desc in TempData, to save it later with localisation form data
        /// return files desc in json format
        /// </summary>
        /// <param name="fileName">The of the file to delete from server</param>
        /// <returns>null</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult DeleteImage(string fileName)
        {
            //to put users in web.config
			//var destinationFolder = Server.MapPath("/Users");
			//var path = Path.Combine(destinationFolder, fileName);
			//try
			//{
			//    //avoid deleting the file for the moment...
			//    //System.IO.File.Delete(path);
			//}
			//catch (Exception ex)
			//{
			//    _Logger.Error(ex.Message);
			//}
            return null;
        }

        /// <summary>
        /// GET Action result to delete a localisation
        /// if the id is in db, ask for confirmation to delete the localiosation
        /// </summary>
        /// <param name="id">The id of the localisation to delete</param>
        /// <returns>the confirmation view</returns>
        [Authorize]
        [ActionName("supprimer")]
        public virtual ActionResult Delete(int id, string returnUrl = null)
        {
            var localisation = _LocalisationRepository.Get(id);
            if (localisation == null)
                return View(MVC.Localisation.Views.lieu_absent);
            else
            {
                TempData["returnUrl"] = returnUrl;
                return View(localisation);
            }
        }

        /// <summary>
        /// POST Action result to delete a localisation
        /// remove localistion from db
        /// <param name="id">The id of the localisation to delete</param>
        /// </summary>
        /// <returns>the deletetion success view</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
        [ActionName("supprimer")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Delete(int id, string confirmButton, string returnUrl)
        {
            var localisation = _LocalisationRepository.Get(id);
            if (localisation == null)
                return View(MVC.Localisation.Views.lieu_absent);
            _LocalisationRepository.Delete(id);
            if (string.IsNullOrEmpty(returnUrl))
                return View(MVC.Localisation.Views.supprimer_reussi);
            else
                return Redirect(returnUrl);
        }

        const string returnUrlPostComment = "returnUrlPostComment";

        /// <summary>
        /// POST Action result to post a comment on a localisation
        /// </summary>
        /// <param name="id">The id of the comment's localisation</param>
        /// <param name="com">The comment data from the form</param>
        /// <returns>redirect to the return urlif ok, show errors else</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
        //[ValidateAntiForgeryToken]
		[HandleModelStateException]
        public virtual PartialViewResult PostComment(int id,Comment com)
        {
			var error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
			var localisation = _LocalisationRepository.Get(id);
			try
			{
				var member = _MemberRepository.GetMember(User.Identity.Name);
				if (!member.IsValidUser())
				{
					error = Worki.Resources.Validation.ValidationString.InvalidUser;
					throw new Exception(error);
				}

				if (ModelState.IsValid)
				{
					com.Localisation = localisation;
					com.Validate(ref  error);
					var comment = new Comment { LocalisationID = id, Date = System.DateTime.Now, PostUserID = member.MemberId };
					UpdateModel(comment);
					var comId = 0;
					_LocalisationRepository.Update(localisation.ID, loc =>
					{
						loc.Comments.Add(comment);
						comId = comment.ID;
					});
					var comFromDb = _LocalisationRepository.GetComment(comment.ID);
					if (comFromDb == null)
						throw new ModelStateException(ModelState);
					return PartialView(MVC.Shared.Views._LocalisationSingleComment, comFromDb);
				}
				else
				{
					throw new ModelStateException(ModelState);
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("PostComment", ex);
				ModelState.AddModelError("", error);
				throw new ModelStateException(ModelState);
			}
        }

        /// <summary>
        /// GET Action result to delete a comment, only available for admin role
        /// redirect to localisation admin page
        /// </summary>
        /// <param name="id">The id of the comment's localisation</param>
        /// <param name="commentId">The id of the comment</param>
        /// <param name="returnUrl">The comment data from the form</param>
        /// <returns>redirect to the return url if ok, show errors else</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminRole)]
        public virtual ActionResult DeleteComment(int id, int commentId, string returnUrl)
        {
            try
            {
                _LocalisationRepository.Update(id, localisation =>
                {
                    foreach (var comment in localisation.Comments.ToList())
                    {
                        if (comment.ID == commentId)
                        {
                            localisation.Comments.Remove(comment);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.Message);
            }
            return Redirect(returnUrl);
        }
    }
}
