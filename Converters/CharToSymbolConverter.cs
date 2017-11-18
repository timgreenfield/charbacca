using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Charbacca.Converters
{
    public class CharToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            var c = (char)value;
            var symbol = c.ToSymbolEnum();
            if (symbol.HasValue) return symbol.Value;
            else return "Undefined";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var name = (string)value;
            Windows.UI.Xaml.Controls.Symbol result;
            if (Enum.TryParse<Windows.UI.Xaml.Controls.Symbol>(name, true, out result))
            {
                int i = (int)result;
                return System.Convert.ToChar(i);
            }
            else
            {
                return default(char);
            }
        }
    }
}
