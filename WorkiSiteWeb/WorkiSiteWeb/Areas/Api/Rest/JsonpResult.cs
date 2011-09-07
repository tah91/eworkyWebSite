using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Worki.Rest
{
    public class JsonpResult : System.Web.Mvc.JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;

            if (!String.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }
            else
            {
                response.ContentType = "application/javascript";
            }
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data != null)
            {
                // The JavaScriptSerializer type was marked as obsolete prior to .NET Framework 3.5 SP1
#pragma warning disable 0618
                HttpRequestBase request = context.HttpContext.Request;

                var serializer = new JavaScriptSerializer();
                response.Write(request.Params["jsoncallback"] + "(" + serializer.Serialize(Data) + ")");
#pragma warning restore 0618
            }
        }
    }
}