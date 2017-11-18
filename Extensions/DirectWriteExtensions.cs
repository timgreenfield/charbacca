using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite.Extensions
{
    public static class DirectWriteExtensions
    {
        public static IEnumerable<FontFamily> GetFontFamilies(this FontCollection fontCollection)
        {
            for (int i = 0; i < fontCollection.FontFamilyCount; i++)
            {
                yield return fontCollection.GetFontFamily(i);
            }
        }
        public static Font GetDefaultFont(this FontFamily fontFamily)
        {
            return fontFamily.GetFirstMatchingFont(SharpDX.DirectWrite.FontWeight.Regular, SharpDX.DirectWrite.FontStretch.Normal, SharpDX.DirectWrite.FontStyle.Normal);
        }

        public static UnicodeRange[] GetUnicodeRanges(this Font1 font1)
        {
            var ranges = new UnicodeRange[0xffff];
            int actualCount;
            font1.GetUnicodeRanges(ranges.Length, ranges, out actualCount);
            return ranges.Take(actualCount).ToArray();
        }

        public static string GetFontFamilyName(this FontFamily fontFamily)
        {
            var defaultLocale = System.Globalization.CultureInfo.CurrentUICulture.Name;
            int localeIndex;
            if (fontFamily.FamilyNames.FindLocaleName(defaultLocale, out localeIndex) == new RawBool(true))
            {
                return fontFamily.FamilyNames.GetString(localeIndex);
            }
            else if (fontFamily.FamilyNames.FindLocaleName("en-us", out localeIndex) == new RawBool(true))
            {
                return fontFamily.FamilyNames.GetString(localeIndex);
            }
            else
            {
                return fontFamily.FamilyNames.GetString(0);
            }
        }
    }
}
