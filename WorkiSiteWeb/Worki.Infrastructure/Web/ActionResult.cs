using System.Web.Mvc;
using System.Collections.Generic;

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

    public interface IObjectStore
    {
        void Delete(string key);
        T Get<T>(string key);
        void Store<T>(string key, T value);
        IList<T> GetList<T>(string key);
    }
}