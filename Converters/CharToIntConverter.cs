using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Charbacca.Converters
{
    public class CharToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            var c = (char)value;
            return System.Convert.ToInt32(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var stringValue = (string)value;
            try
            {
                return stringValue.FromIntToChar();
            }
            catch
            {
                return default(char);
            }
        }
    }
}
