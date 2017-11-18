using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Charbacca.Converters
{
    public class CharToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            var c = (char)value;
            return c.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var stringValue = (string)value;
            char c;
            if (char.TryParse(stringValue, out c))
            {
                return c;
            }
            return default(char);
        }
    }
}
