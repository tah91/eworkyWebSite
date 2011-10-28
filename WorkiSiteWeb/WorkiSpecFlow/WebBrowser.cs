using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WatiN.Core;
using TechTalk.SpecFlow;

namespace Worki.SpecFlow
{
    public static class WebBrowser
    {
        public const string RootURL = "http://localhost:2441/"; 

        public static IE Current
        {
            get
            {
                string key = "browser";
                if (!ScenarioContext.Current.ContainsKey(key))
                {
                    ScenarioContext.Current[key] = new IE();
                }
                return ScenarioContext.Current[key] as IE;
            }
        }

        /// <summary>
        /// Sets text quickly, but does not raise key events or focus/blur events
        /// Source: http://blog.dbtracer.org/2010/08/05/speed-up-typing-text-with-watin/
        /// </summary>
        /// <param name="textField"></param>
        /// <param name="text"></param>
        public static void TypeTextQuickly(this TextField textField, string text)
        {
            textField.SetAttributeValue("value", text);
        }

    }
}
