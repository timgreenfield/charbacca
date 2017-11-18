using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Charbacca
{
    public static class CharExtensions
    {
        public static char FromUnicodeName(this string value)
        {
            var character = Data.UnicodeData.Characters.FirstOrDefault(c => c.AllNames.Any(n => string.Compare(n, value, StringComparison.CurrentCultureIgnoreCase) == 0));
            if (character != null)
            {
                return Convert.ToChar(character.CodePageInt);
            }
            throw new ArgumentException();
        }

        public static string ToUnicodeName(this char value)
        {
            var i = Convert.ToInt32(value);
            var character = Data.UnicodeData.Characters.FirstOrDefault(c => c.CodePageInt == i);
            if (character != null)
            {
                return character.BestName;
            }
            return null;
        }

        public static Windows.UI.Xaml.Controls.Symbol? ToSymbolEnum(this char value)
        {
            var i = Convert.ToInt32(value);
            if (Enum.IsDefined(typeof(Windows.UI.Xaml.Controls.Symbol), i))
            {
                return (Windows.UI.Xaml.Controls.Symbol)i;
            }
            return null;
        }

        public static string ToHex(this char value, int requireChars = 4)
        {
            return Convert.ToInt32(value).ToHex(requireChars);
        }

        public static string ToHex(this int value, int requireChars = 4)
        {
            var hex = value.ToString("X");
            return string.Format("{0}{1}", new string('0', requireChars - hex.Length), hex);
        }

        public static int FromHexToChar(this string hex)
        {
            return Convert.ToChar(hex.FromHexToInt());
        }

        public static int FromHexToInt(this string hex)
        {
            return Convert.ToInt32(hex, 16);
        }

        public static char FromIntToChar(this string number)
        {
            return Convert.ToChar(int.Parse(number));
        }
    }
}
