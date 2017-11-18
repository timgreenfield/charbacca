using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Search;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Charbacca
{
    public sealed partial class CharacterView : UserControl
    {
        double imageAspectRatio;
        Task<List<Models.Character>> unicodeCharactersTask; // cache this for performance reasons

        public CharacterView()
        {
            this.InitializeComponent();
            characterIcon.SizeChanged += CharacterIcon_SizeChanged;
            textWidth.LostFocus += TextWidth_LostFocus;
            textHeight.LostFocus += TextHeight_LostFocus;
            DataContextChanged += CharacterView_DataContextChanged;
        }

        void CharacterView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                var vm = ViewModel;
                unicodeCharactersTask = Task.Run(() =>
                {
                    var characters = vm.Characters.ToList();
                    return Data.UnicodeData.Characters.Where(c => characters.Contains((char)c.CodePageInt)).ToList();
                });

                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Characters":
                    var vm = ViewModel;
                    unicodeCharactersTask = Task.Run(() =>
                    {
                        var characters = vm.Characters.ToList();
                        return Data.UnicodeData.Characters.Where(c => characters.Contains((char)c.CodePageInt)).ToList();
                    });
                    break;
            }
        }

        void TextHeight_LostFocus(object sender, RoutedEventArgs e)
        {
            textWidth.Text = Math.Round(ImageHeight * imageAspectRatio).ToString();
        }

        void TextWidth_LostFocus(object sender, RoutedEventArgs e)
        {
            textHeight.Text = Math.Round(ImageWidth / imageAspectRatio).ToString();
        }

        void CharacterIcon_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            imageAspectRatio = e.NewSize.Width / e.NewSize.Height;
            textWidth.Text = Math.Round(e.NewSize.Width).ToString();
            textHeight.Text = Math.Round(e.NewSize.Height).ToString();
        }

        int ImageHeight
        {
            get
            {
                int result;
                int.TryParse(textHeight.Text, out result);
                return result;
            }
        }

        int ImageWidth
        {
            get
            {
                int result;
                int.TryParse(textWidth.Text, out result);
                return result;
            }
        }

        ViewModel ViewModel
        {
            get { return this.DataContext as ViewModel; }
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var data = button.CommandParameter as string;
            var dataPackage = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            dataPackage.SetText(data);
            Clipboard.SetContent(dataPackage);
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedCharacter == null) return;

            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                DefaultFileExtension = ".png"
            };
            picker.FileTypeChoices.Add("PNG Format", new string[] { ".png" });

            var symbol = ViewModel.SelectedCharacter.Value.ToSymbolEnum();
            picker.SuggestedFileName = (symbol.HasValue ? symbol.Value.ToString() : ViewModel.SelectedCharacter.Value.ToUnicodeName()) ?? "Undefined";

            var dest = await picker.PickSaveFileAsync();
            if (dest != null)
            {
                var renderTargetBitmap = new RenderTargetBitmap();
                await renderTargetBitmap.RenderAsync(characterIcon, ImageWidth, ImageHeight);
                var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

                using (var destStream = await dest.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, destStream);

                    var dpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                    encoder.SetPixelData(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight,
                        (uint)renderTargetBitmap.PixelWidth,
                        (uint)renderTargetBitmap.PixelHeight,
                        dpi,
                        dpi,
                        pixelBuffer.ToArray());

                    await encoder.FlushAsync();
                }
            }
        }

        private void TextEnum_SuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.QueryText))
            {
                var suggestions = ViewModel.Characters
                    .Select(c => c.ToSymbolEnum())
                    .Where(v => v.HasValue)
                    .Select(v => v.Value.ToString())
                    .Where(s => s.StartsWith(args.QueryText, StringComparison.CurrentCultureIgnoreCase));

                args.Request.SearchSuggestionCollection.AppendQuerySuggestions(suggestions);
            }
        }

        private async void TextName_SuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.QueryText))
            {
                var characters = ViewModel.Characters.ToList();

                //var suggestions = Data.UnicodeData.Characters
                //    .Where(c => characters.Contains((char)c.CodePageInt))
                //    .SelectMany(c => c.AllNames)
                //    .Where(n => n.StartsWith(args.QueryText, StringComparison.CurrentCultureIgnoreCase))
                //    .ToList();

                // used cache list of filtered unicode character objects. This is too expensive to do everytime the user types in a character.
                List<Models.Character> unicodeCharacters;
                SearchSuggestionsRequestDeferral deferral = null;

                if (!unicodeCharactersTask.IsCompleted)
                {
                    deferral = args.Request.GetDeferral();
                    unicodeCharacters = await unicodeCharactersTask;
                }
                else
                {
                    unicodeCharacters = unicodeCharactersTask.Result;
                }

                var suggestions = unicodeCharacters
                    .SelectMany(c => c.AllNames)
                    .Where(n => n.StartsWith(args.QueryText, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();

                args.Request.SearchSuggestionCollection.AppendQuerySuggestions(suggestions);

                if (deferral != null) deferral.Complete();
            }
        }
    }
}
