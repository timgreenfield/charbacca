using Charbacca.Common;
using Charbacca.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Charbacca
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        Font selectedFont;
        char? selectedCharacter;
        readonly RelayCommand copyCommand;
        readonly RelayCommand settingsCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            copyCommand = new RelayCommand(CopySelectedCharacter, () => SelectedCharacter.HasValue);
            settingsCommand = new RelayCommand(ToggleTheme);
        }

        void ToggleTheme()
        {
            MessageService.Current.CurrentTheme = IsDarkTheme ? ElementTheme.Light : ElementTheme.Dark;
            OnPropertyChanged("IsDarkTheme");
        }

        public bool IsDarkTheme => MessageService.Current.CurrentTheme == ElementTheme.Dark;

        public virtual async Task LoadAsync(Dictionary<string, object> pageState, object navigationParam)
        {
            await Data.InitAsync();

            AllFonts = Data.Fonts;

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Character"))
            {
                selectedCharacter = (char)ApplicationData.Current.LocalSettings.Values["Character"];
            }

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Font"))
            {
                SelectedFont = AllFonts.First(f => f.Name == (string)ApplicationData.Current.LocalSettings.Values["Font"]);
            }
            else
            {
                selectedCharacter = '!';
                SelectedFont = AllFonts.First(f => f.Name == "Segoe MDL2 Assets");
            }
        }

        public virtual void Save(Dictionary<string, object> pageState)
        { }

        public IList<Font> AllFonts { get; private set; }

        public Font SelectedFont
        {
            get { return selectedFont; }
            set
            {
                selectedFont = value;
                OnPropertyChanged("SelectedFont");
                if (selectedFont != null)
                {
                    ApplicationData.Current.LocalSettings.Values["Font"] = selectedFont.Name;
                    UpdateSelectedFont();
                }
            }
        }

        protected virtual void UpdateSelectedFont()
        { }

        public char? SelectedCharacter
        {
            get { return selectedCharacter; }
            set
            {
                if (selectedCharacter != value && value != default(char))
                {
                    selectedCharacter = value;
                    OnPropertyChanged("SelectedCharacter");
                    if (selectedCharacter.HasValue)
                    {
                        ApplicationData.Current.LocalSettings.Values["Character"] = selectedCharacter;
                    }
                    else if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Character"))
                    {
                        ApplicationData.Current.LocalSettings.Values.Remove("Character");
                    }
                    copyCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public abstract IEnumerable<char> Characters { get; }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        void CopySelectedCharacter()
        {
            var dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(SelectedCharacter.ToString());
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }

        public ICommand CopyCommand
        {
            get { return copyCommand; }
        }

        public ICommand SettingsCommand
        {
            get { return settingsCommand; }
        }
    }
}
