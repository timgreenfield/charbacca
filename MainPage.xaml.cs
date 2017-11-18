using System;
using System.Linq;
using Charbacca.Common;
using Charbacca.Models;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Charbacca
{
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private bool ignoreSelectedRangeChange;

        public MainPage()
        {
            this.InitializeComponent();

            var vsGroup = VisualStateManager.GetVisualStateGroups(LayoutRoot).First(vs => vs.Name == "SelectionStates");
            vsGroup.CurrentStateChanged += MainPage_CurrentStateChanged;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += NavigationHelper_LoadState;
            this.navigationHelper.SaveState += NavigationHelper_SaveState;
            gridView.SelectionChanged += GridView_SelectionChanged;
            semanticZoom.ViewChangeCompleted += SemanticZoom_ViewChangeCompleted;
            this.RequestedTheme = MessageService.Current.CurrentTheme;
            MessageService.Current.CurrentThemeChanged += Current_CurrentThemeChanged;
        }

        void SemanticZoom_ViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.IsSourceZoomedInView)
            {
                gridView.SelectedItem = null;
            }
            else
            {
                //gridView.SelectedItem = e.DestinationItem.Item; 
            }
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
            var vm = new MainViewModel();
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

        public MainViewModel ViewModel
        {
            get { return this.DataContext as MainViewModel; }
        }

        private void Flyout_Opened(object sender, object e)
        {
            RangeSelector.ItemsSource = ViewModel.AllRanges;
            UpdateFlyoutRanges();
        }

        private void Flyout_Closed(object sender, object e)
        {
            ignoreSelectedRangeChange = true;
            RangeSelector.SelectedItems.Clear();
            ignoreSelectedRangeChange = false;
            RangeSelector.ItemsSource = null;
            //BottomAppBar.IsOpen = false;
        }

        private void UpdateFlyoutRanges()
        {
            ignoreSelectedRangeChange = true;
            RangeSelector.SelectedItems.Clear();
            foreach (var range in ViewModel.SelectedRanges)
            {
                RangeSelector.SelectedItems.Add(range);
            }
            ignoreSelectedRangeChange = false;
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectDefaultRanges();
            UpdateFlyoutRanges();
        }

        private void RangeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelectedRangeChange) return;
            ViewModel.SelectedRanges = RangeSelector.SelectedItems.Cast<Range>().OrderBy(r => r.Start).ToList();
        }

        private void Header_Click(object sender, RoutedEventArgs e)
        {
            // Determine what group the Button instance represents
            var group = (sender as FrameworkElement).DataContext;

            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            this.Frame.Navigate(typeof(GroupDetailPage), ((Range)group).Name);
        }
    }
}
