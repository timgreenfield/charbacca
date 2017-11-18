using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Charbacca.Converters
{
    public class CharToNameConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            var c = (char)value;
            return c.ToUnicodeName() ?? "Undefined";
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var s = (string)value;
                return s.FromUnicodeName();
            }
            catch
            {
                return default(char);
            }
        }
    }
}
