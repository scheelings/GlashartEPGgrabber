using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlashartLibrary.Helpers
{
    public sealed class HtmlHelper
    {
        /// <summary>
        /// Gets the script tag SRC attribute.
        /// </summary>
        /// <param name="htmlFile">The HTML file.</param>
        /// <returns></returns>
        public static string GetScriptTagSrc(string htmlFile)
        {
            HtmlDocument html = new HtmlDocument();
            html.Load(htmlFile);

            //Get all Nodes with src or href in the TAG
            HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("//*[@src]");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (HtmlNode node in nodes)
                {
                    //Get the needed attributes
                    HtmlAttribute typeAttr = node.Attributes["type"];
                    HtmlAttribute srcAttr = node.Attributes["src"];

                    if (typeAttr.Value.IndexOf("javascript", StringComparison.InvariantCultureIgnoreCase) != -1)
                        return srcAttr.Value;
                }
            }

            return null;
        }
    }
}
