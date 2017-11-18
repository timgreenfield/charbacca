using Charbacca.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Charbacca
{
    public class MainViewModel : ViewModel
    {
        IList<Range> selectedRanges;
        IList<string> restoreRanges;

        public override async Task LoadAsync(Dictionary<string, object> pageState, object navigationParam)
        {
            await base.LoadAsync(pageState, navigationParam);

            if (pageState != null && pageState.ContainsKey("SelectedRanges"))
            {
                restoreRanges = ((string)pageState["SelectedRanges"]).Split(';');
            }
            else
            {
                restoreRanges = null;
            }
            
            try
            {
                // Potentially ask the user if they want to rate the app.
                await RatingNotifier.TriggerNotificationAsync("Like Charbacca?", "Would you like to rate Charbacca in the store?", "Yes", "No thanks", "Maybe later", 5, 5);
            }
            catch
            { 
                // just in case, we never want this to crash the app
            }
        }

        public override void Save(Dictionary<string, object> pageState)
        {
            base.Save(pageState);
            pageState["SelectedRanges"] = string.Join(";", SelectedRanges.Select(r => r.Name));
        }

        protected override void UpdateSelectedFont()
        {
            base.UpdateSelectedFont();

            AllRanges = SelectedFont.Ranges;
            if (restoreRanges != null)
            {
                SelectedRanges = AllRanges.Where(r => restoreRanges.Contains(r.Name)).ToList();
                restoreRanges = null;
            }
            else
            {
                SelectDefaultRanges();
            }
        }

        public void SelectDefaultRanges()
        {
            SelectedRanges = AllRanges.Where(r => !r.IsEmpty).OrderBy(r => r.Start).ToList();
        }

        public IList<Range> AllRanges { get; private set; }

        public IList<Range> SelectedRanges
        {
            get { return selectedRanges; }
            set
            {
                if (selectedRanges == null || !selectedRanges.SequenceEqual(value))
                {
                    var myChar = SelectedCharacter;

                    selectedRanges = value;
                    OnPropertyChanged("SelectedRanges");
                    OnPropertyChanged("Characters");

                    if (!myChar.HasValue)
                    {
                        SelectedCharacter = myChar;
                    }
                    else if (!selectedRanges.SelectMany(r => r.Characters).Contains(myChar.Value))
                    {
                        SelectedCharacter = null;
                        //var firstRange = selectedRanges.FirstOrDefault();
                        //if (firstRange != null)
                        //{
                        //    SelectedCharacter = firstRange.Characters.FirstOrDefault();
                        //}
                    }
                    else
                    {
                        SelectedCharacter = myChar;
                    }
                }
            }
        }

        public override IEnumerable<char> Characters
        {
            get { return SelectedRanges.SelectMany(r => r.Characters); }
        }
    }
}
