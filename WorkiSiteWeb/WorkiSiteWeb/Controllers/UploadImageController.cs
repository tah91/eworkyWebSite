﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Worki.Web.Helpers;
using Worki.Data.Models;
using System.Collections.Generic;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure;
using Worki.Infrastructure.Helpers;

namespace Worki.Web.Controllers
{
    public partial class UploadImageController : Controller
    {
        ILogger _Logger;
        IObjectStore _ObjectStore;

        public UploadImageController(ILogger logger, IObjectStore objectStore)
		{
			_Logger = logger;
            _ObjectStore = objectStore;
		}

        IPictureDataProvider GetProvider(ProviderType type, int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            switch (type)
            {
                case ProviderType.Rental:
                    {
                        var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
                        return rRepo.Get(id);
                    }
                case ProviderType.Offer:
                    {
                        var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
                        return oRepo.Get(o => o.Id == id);
                    }
                case ProviderType.Localisation:
                default:
                    {
                        var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
                        return lRepo.Get(id);
                    }
            }
        }

        /// <summary>
        /// Action method to handle the json upload of files
        /// </summary>
        /// <returns>json result containing uploaded data</returns>
        [AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult UploadFiles(ProviderType type)
        {
            var toRet = new List<ImageJson>();
            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
			var folder = PictureData.GetFolder(type);
            foreach (string name in Request.Files)
            {
                try
                {
                    var postedFile = Request.Files[name];
                    if (postedFile == null || string.IsNullOrEmpty(postedFile.FileName))
                        continue;
                    var uploadedFileName = this.UploadFile(postedFile, MiscHelpers.ImageSize.Localisation, folder);
					var url = ControllerHelpers.GetUserImagePath(uploadedFileName, true, folder);
                    var deleteUrl = urlHelper.Action(MVC.UploadImage.DeleteImage(uploadedFileName));

                    toRet.Add(new ImageJson
                    {
                        name = uploadedFileName,
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
					_Logger.Error("UploadFiles", ex);
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
        public virtual ActionResult LoadFiles(int id, ProviderType type)
        {
            var toRet = new List<ImageJson>();
            //to recover files state in case of error
            var filesFromCache = _ObjectStore.Get<PictureDataContainer>(PictureData.GetKey(type));

            if (filesFromCache == null)
            {
                var provider = GetProvider(type, id);
                if (provider == null)
                    return Json(toRet, "text/html");
                filesFromCache = new PictureDataContainer(provider);
            }

            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
			var folder = PictureData.GetFolder(type);
            foreach (var file in filesFromCache.Files)
            {
				var url = ControllerHelpers.GetUserImagePath(file.FileName, true, folder);
                var deleteUrl = urlHelper.Action(MVC.UploadImage.DeleteImage(file.FileName));
                toRet.Add(new ImageJson
                {
                    name = file.FileName,
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
    }
}
