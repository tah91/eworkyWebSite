using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Reflection;

namespace Worki.Infrastructure
{
    public class FileUploadJsonResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            this.ContentType = "text/html";
            context.HttpContext.Response.Write("<textarea>");
            base.ExecuteResult(context);
            context.HttpContext.Response.Write("</textarea>");
        }
    }
}