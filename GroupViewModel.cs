using Charbacca.Common;
using Charbacca.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Charbacca
{
    public class GroupViewModel : ViewModel
    {
        Range range;
        string rangeName;

        public GroupViewModel(string rangeName)
            : base()
        {
            this.rangeName = rangeName;
        }

        protected override void UpdateSelectedFont()
        {
            var myChar = SelectedCharacter;

            base.UpdateSelectedFont();

            Range = SelectedFont.Ranges.First(r => r.Name == rangeName);

            if (!myChar.HasValue)
            {
                SelectedCharacter = myChar;
            }
            else if (!Range.Characters.Contains(myChar.Value))
            {
                SelectedCharacter = null;
                //SelectedCharacter = Range.Characters.FirstOrDefault();
            }
            else
            {
                SelectedCharacter = myChar;
            }
        }
        
        public Range Range
        {
            get { return range; }
            set
            {
                if (range != value)
                {
                    range = value;
                    OnPropertyChanged("Range");
                    OnPropertyChanged("Characters");
                }
            }
        }

        public override IEnumerable<char> Characters
        {
            get { return Range.Characters; }
        }
    }
}
