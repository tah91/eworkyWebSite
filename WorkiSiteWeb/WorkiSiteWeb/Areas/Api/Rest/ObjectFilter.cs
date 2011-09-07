using System;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using System.IO;
using System.Xml.Serialization;

namespace Worki.Rest
{
    public class ObjectFilter : ActionFilterAttribute
    {
        public string Param { get; set; }
        public Type RootType { get; set; }

        #region IActionFilter Members

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if ((filterContext.HttpContext.Request.ContentType ?? string.Empty).Contains("application/json"))
            {
                //object o = new DataContractJsonSerializer(RootType)
                //    .ReadObject(filterContext.HttpContext.Request.InputStream);

                //string json = new StreamReader(HttpContext.Current.Request.InputStream).ReadToEnd();
                //var serializer = new JavaScriptSerializer();
                //object deserializedObject = serializer.Deserialize(json, RootType);
                //filterContext.ActionParameters[Param] = deserializedObject;
            }
            else
            {
                var xmlRoot = XElement.Load(new StreamReader(filterContext.HttpContext.Request.InputStream, filterContext.HttpContext.Request.ContentEncoding));

                object o = new XmlSerializer(RootType).Deserialize(xmlRoot.CreateReader());
                filterContext.ActionParameters[Param] = o;
            }
        }

        #endregion
    }
}