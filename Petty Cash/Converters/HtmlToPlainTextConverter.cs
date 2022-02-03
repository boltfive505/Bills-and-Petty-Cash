using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Net;
using HtmlAgilityPack;

namespace Petty_Cash.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class HtmlToPlainTextConverter : IValueConverter
    {
        private static Dictionary<string, string> _htmlEntities = new Dictionary<string, string>()
        {
            { "&nbsp;", " " },
            { "&lt;", "<" },
            { "&gt;", ">" },
            { "&amp;", "&" },
            { "&quot;", "\"" },
            { "&apos;", "'" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string html = System.Convert.ToString(value);
            html = html.Replace("\r", "").Replace("\n", "");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            string txt = doc.DocumentNode.InnerText; //get pure text from html -> removes tags
            txt = Regex.Replace(txt, string.Join("|", _htmlEntities.Keys), m => _htmlEntities[m.Value]); //parse html entities

            return txt;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
