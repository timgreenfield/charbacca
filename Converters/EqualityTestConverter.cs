using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Charbacca.Converters
{
    public class EqualityTestConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return object.Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
