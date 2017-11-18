using System;
using System.Linq;
using Charbacca.Common;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Charbacca
{
    public sealed partial class GroupDetailPage : Page
    {
        private NavigationHelper navigationHelper;
        
        public GroupDetailPage()
        {
            this.InitializeComponent();

            var vsGroup = VisualStateManager.GetVisualStateGroups(LayoutRoot).First(vs => vs.Name == "SelectionStates");
            vsGroup.CurrentStateChanged += MainPage_CurrentStateChanged;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += NavigationHelper_LoadState;
            this.navigationHelper.SaveState += NavigationHelper_SaveState;
            gridView.SelectionChanged += GridView_SelectionChanged;
            this.RequestedTheme = MessageService.Current.CurrentTheme;
            MessageService.Current.CurrentThemeChanged += Current_CurrentThemeChanged;
        }

        void Current_CurrentThemeChanged(object sender, System.EventArgs e)
        {
            this.RequestedTheme = MessageService.Current.CurrentTheme;
        }

        async void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Any() && !e.RemovedItems.Any())
            {
                VisualStateManager.GoToState(this, "Selected", true);
            }
            else if (!e.AddedItems.Any() && e.RemovedItems.Any())
            {
                VisualStateManager.GoToState(this, "None", true);
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (gridView.SelectedItem != null)
                    {
                        gridView.ScrollIntoView(gridView.SelectedItem);
                    }
                });
            }
        }
        void MainPage_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (gridView.SelectedItem != null)
            {
                gridView.ScrollIntoView(gridView.SelectedItem, ScrollIntoViewAlignment.Default);
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var groupName = (string)e.NavigationParameter;
            var vm = new GroupViewModel(groupName);
            VisualStateManager.GoToState(this, "Busy", true);
            await vm.LoadAsync(e.PageState, e.NavigationParameter);
            this.DataContext = vm;
            VisualStateManager.GoToState(this, "Ready", true);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            ViewModel.Save(e.PageState);
        }
        public ViewModel ViewModel
        {
            get { return this.DataContext as ViewModel; }
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
