using System;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Charbacca
{
    public class MessageService
    {
        public MessageService()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Theme"))
            {
                currentTheme = (ElementTheme)ApplicationData.Current.LocalSettings.Values["Theme"];
            }
            else
            {
                currentTheme = ElementTheme.Dark;
            }
        }

        public event EventHandler CurrentThemeChanged;

        ElementTheme currentTheme;
        public ElementTheme CurrentTheme
        {
            get { return currentTheme; }
            set
            {
                if (currentTheme != value)
                {
                    currentTheme = value;
                    CurrentThemeChanged?.Invoke(this, EventArgs.Empty);
                    ApplicationData.Current.LocalSettings.Values["Theme"] = (int)CurrentTheme;
                }
            }
        }

        static readonly MessageService current = new MessageService();
        public static MessageService Current
        {
            get { return current; }
        }
    }
}
