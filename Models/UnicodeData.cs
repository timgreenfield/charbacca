using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Charbacca.Models
{
    [XmlRoot("ucd")]
    public class UnicodeData
    {
        [XmlArray("repertoire")]
        [XmlArrayItem(typeof(Character), ElementName = "char")]
        public List<Character> Characters { get; set; }

        [XmlArray("blocks")]
        [XmlArrayItem(typeof(Block), ElementName = "block")]
        public List<Block> Blocks { get; set; }

        public static UnicodeData Load(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(UnicodeData), "http://www.unicode.org/ns/2003/ucd/1.0");
            return serializer.Deserialize(stream) as UnicodeData;
        }

#if DEBUG
        public static void Save(Stream stream, UnicodeData data)
        {
            var serializer = new XmlSerializer(typeof(UnicodeData), "http://www.unicode.org/ns/2003/ucd/1.0");
            serializer.Serialize(stream, data);
            stream.Flush();
        }
#endif
    }

    [XmlRoot("char")]
    public class Character
    {
        [XmlAttribute("cp")]
        public string CodePage { get; set; }

        [XmlAttribute("na")]
        public string Name { get; set; }

        [XmlElement("name-alias")]
        public List<Alias> Aliases { get; set; }

        [XmlIgnore]
        public int CodePageInt { get { return CodePage.FromHexToInt(); } }

        [XmlIgnore]
        public string BestName
        {
            get
            {
                if (!string.IsNullOrEmpty(Name)) return Name;
                return Aliases.OrderBy(a => a.Type).Select(a => a.Name).FirstOrDefault();
            }
        }

        [XmlIgnore]
        public IEnumerable<string> AllNames
        {
            get
            {
                if (!string.IsNullOrEmpty(Name)) yield return Name;
                foreach (var alias in Aliases)
                {
                    yield return alias.Name;
                }
            }
        }
    }

    [XmlRoot("name-alias")]
    public class Alias
    {
        [XmlAttribute("alias")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public AliasType Type { get; set; }
    }

    public enum AliasType
    {
        [XmlEnum("abbreviation")]
        Abbreviation = 3,
        [XmlEnum("control")]
        Control = 0,
        [XmlEnum("figment")]
        Figment = 1,
        [XmlEnum("correction")]
        Correction = 5,
        [XmlEnum("alternate")]
        Alternate = 2
    }

    [XmlRoot("block")]
    public class Block
    {
        [XmlAttribute("first-cp")]
        public string Start { get; set; }

        [XmlAttribute("last-cp")]
        public string End { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlIgnore]
        public int StartInt { get { return Start.FromHexToInt(); } }

        [XmlIgnore]
        public int EndInt { get { return End.FromHexToInt(); } }
    }
}
