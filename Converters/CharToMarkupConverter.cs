using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Charbacca.Converters
{
    public class CharToMarkupConverter : CharToHexConverter
    {
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            var result = base.Convert(value, targetType, parameter, language);
            if (result == null) return null;
            return string.Format("&#x{0};", result);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string markup = (string)value;
            var markdown = markup;
            if (markdown.StartsWith("&#x"))
            {
                markdown = markdown.Substring(3);
            }
            if (markdown.EndsWith(";"))
            {
                markdown = markdown.Substring(0, markdown.Length - 1);
            }
            return base.ConvertBack(markdown, targetType, parameter, language);
        }
    }
}
