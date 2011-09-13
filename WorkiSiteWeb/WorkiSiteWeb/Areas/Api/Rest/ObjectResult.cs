using System;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Worki.Rest
{
    public class Output<T>
    {
        public const int DefaultStatus = 200;
        public const string DefaultMessage = "SUCCESS";

        public class MetaContent
        {
            public int statusCode { get; set; }
            public string message { get; set; }
        }

        public MetaContent meta { get; set; }
        public T response { get; set; }
    }

    public class ObjectResult<T> : ActionResult
    {
        private static UTF8Encoding UTF8 = new UTF8Encoding(false);

        public Output<T> Data { get; set; }

        public Type[] IncludedTypes = new[] { typeof(object) };

        public ObjectResult(T data, int statusCode, string message, Type[] extraTypes = null)
        {
            this.Data = new Output<T> { meta = new Output<T>.MetaContent { statusCode = statusCode, message = message }, response = data };
            if (extraTypes != null)
                this.IncludedTypes = extraTypes;
        }

        public ObjectResult(T data, Type[] extraTypes = null)
        {
            this.Data = new Output<T> { meta = new Output<T>.MetaContent { statusCode = Output<T>.DefaultStatus, message = Output<T>.DefaultMessage }, response = data };
            if (extraTypes != null)
                this.IncludedTypes = extraTypes;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            // If ContentType is not expected to be application/json, then return XML
            if (context.HttpContext.Request["jsoncallback"] != null)
            {
                SerializeToJsonp(context);
            }
            else if (context.HttpContext.Request["json"] != null)
            {
                SerializeToJson(context);
            }
            else if ((context.HttpContext.Request.ContentType ?? string.Empty).Contains("application/json"))
            {
                SerializeToJson(context);
            }
            else
            {
                SerializeToXml(context);
            }
        }

        private void SerializeToJson(ControllerContext context)
        {
            var result = JsonConvert.SerializeObject(this.Data, Formatting.Indented);
            var contentResult = new ContentResult
            {
                ContentType = "application/json",
                Content = result,
                ContentEncoding = UTF8
            };
            contentResult.ExecuteResult(context);
        }

        private void SerializeToJsonp(ControllerContext context)
        {
            var format = Formatting.Indented;
            var result = HttpContext.Current.Request.Params["jsoncallback"] + "(" + JsonConvert.SerializeObject(this.Data, format) + ")";
            var contentResult = new ContentResult
            {
                ContentType = "application/javascript",
                Content = result,
                ContentEncoding = UTF8
            };
            contentResult.ExecuteResult(context);
        }

        private void SerializeToXml(ControllerContext context)
        {
            using (var stream = new MemoryStream(500))
            {
                var settings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    Encoding = UTF8,
                    Indent = true
                };
                using (var xmlWriter = XmlTextWriter.Create(stream, settings))
                {
                    var serializer = new XmlSerializer(typeof(Output<T>), IncludedTypes);
                    serializer.Serialize(xmlWriter, this.Data);
                }
                // NOTE: We need to cache XmlSerializer for specific type. Probably use the 
                // GenerateSerializer to generate compiled custom made serializer for specific
                // types and then cache the reference
                var contentResult = new ContentResult
                {
                    ContentType = "text/xml",
                    Content = UTF8.GetString(stream.ToArray()),
                    ContentEncoding = UTF8
                };
                contentResult.ExecuteResult(context);
            }
        }
    }
}