using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Charbacca.Converters
{
    public class CharToHexConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            var c = (char)value;
            return c.ToHex();
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string hex = (string)value;
            try
            {
                return hex.FromHexToChar();
            }
            catch
            {
                return default(char);
            }
        }
    }
}
