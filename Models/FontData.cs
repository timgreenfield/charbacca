using SharpDX.DirectWrite.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using SharpDX.Mathematics.Interop;

namespace Charbacca.Models
{
    public class FontData
    {
        const int MaxCodePage = 0xffff;

        public static async Task<IList<Font>> LoadAsync()
        {
            using (var dwFactory = new SharpDX.DirectWrite.Factory())
            {
                using (var fontCollection = dwFactory.GetSystemFontCollection(new RawBool(true)))
                {
                    return await Task.WhenAll(fontCollection.GetFontFamilies().Select(family =>
                    {
                        return Task.Run(() =>
                        {
                            var familyName = family.GetFontFamilyName();
                            var result = CreateFont(familyName, family);
                            family.Dispose();
                            return result;
                        });
                    }));
                }
            }
        }

        static Font CreateFont(string name, SharpDX.DirectWrite.FontFamily fontFamily)
        {
            var myFont = new Font(name);
            using (var defaultFont = new SharpDX.DirectWrite.Font1(fontFamily.GetDefaultFont().NativePointer))
            {
                var ranges = defaultFont.GetUnicodeRanges();
                myFont.Ranges = new List<Range>();
                foreach (var block in Data.UnicodeData.Blocks.Where(b => b.StartInt <= MaxCodePage))
                {
                    var myRange = new Range(block.Name, block.StartInt, block.EndInt);
                    foreach (var range in ranges.SkipWhile(r => r.Last < myRange.Start).TakeWhile(r => r.First <= myRange.End))
                    {
                        for (int j = Math.Max(block.StartInt, range.First); j < Math.Min(block.EndInt, range.Last); j++)
                        {
                            var c = Convert.ToChar(j);
                            if (!char.IsWhiteSpace(c))
                            {
                                myRange.Characters.Add(c);
                            }
                        }
                    }
                    myFont.Ranges.Add(myRange);
                }
            }
            return myFont;
        }
    }

    public class Font
    {
        FontFamily family;

        public Font(string name)
        {
            Name = name;
        }
        public FontFamily Family
        {
            get
            {
                if (family == null)
                {
                    family = new FontFamily(Name);
                }
                return family;
            }
        }

        public string Name { get; set; }

        public IList<Range> Ranges { get; internal set; }
    }

    public class Range
    {
        public Range(string name, int start, int end)
        {
            Name = name;
            Start = start;
            End = end;
            Characters = new List<char>();
        }

        public IList<char> Characters { get; private set; }
        public string Name { get; private set; }
        public int Start { get; private set; }
        public int End { get; private set; }
        public bool IsEmpty { get { return Characters.Count == 0; } }
    }
}
