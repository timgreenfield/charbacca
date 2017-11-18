using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;

namespace Charbacca
{
    /// <summary>
    /// http://blogs.msdn.com/b/belux/archive/2013/08/30/getting-more-downloads-for-your-windows-store-app.aspx
    /// </summary>
    public class RatingNotifier
    {
        const string NoButton = "None";
        const string IsRatedKey = "IsRated";
        const string RatingRetryKey = "RatingRetry";
        const string RatingCounterKey = "RatingCounter";

        /// <summary>
        /// Triggers the rating reminder logic. Checks if we have surpassed the usage threshold and prompts the user for feedback if appropriate.
        /// </summary>
        public async static Task TriggerNotificationAsync(string title, string message, string yes, string no, string later, int interval, int maxRetry)
        {
            // Initialize settings
            var counter = 0;    // usage counter
            var rated = false;  // indicate if rating has happened
            var retryCount = 0; // number of times we've reminded the user

            try
            {
                // Use roaming app storage to ensure we sync the values across all the user's devices
                var settingsContainer = ApplicationData.Current.RoamingSettings;

                // Retrieve the current values if they are available, otherwise use the initialized values
                if (settingsContainer.Values.ContainsKey(IsRatedKey))
                    rated = Convert.ToBoolean(settingsContainer.Values[IsRatedKey]);
                if (settingsContainer.Values.ContainsKey(RatingRetryKey))
                    retryCount = Convert.ToInt32(settingsContainer.Values[RatingRetryKey]);
                if (settingsContainer.Values.ContainsKey(RatingCounterKey))
                    counter = Convert.ToInt32(settingsContainer.Values[RatingCounterKey]);

                // Increment the usage counter
                counter = counter + 1;

                // Store the current values in roaming app storage
                SaveSettings(rated, counter, retryCount);
            }
            catch
            {
                // TODO: Manage errors here
            }

            // Do we need to ask the user for feedback
            if (!rated &&                                   // user has not rated the app yet or has refused to rate the app
                retryCount < maxRetry &&                    // we have not yet exceeded the max number reminders (e.g. max 3 times)
                counter >= interval * (retryCount + 1))     // we have surpassed the usage threshold for asking the user (e.g. every 15 times)
            {
                // Create a dialog window to ask for feedback
                MessageDialog md = new MessageDialog(message, title);

                // User wants to rate the app
                if (!string.IsNullOrEmpty(yes) && yes != NoButton)
                {
                    md.Commands.Add(new UICommand(yes, async (s) =>
                    {
                        // Store the current values in roaming app storage
                        SaveSettings(true, 0, 0);

                        // Launch the app's review page in the Windows Store using protocol link
                        await Launcher.LaunchUriAsync(new Uri(
                            String.Format("ms-windows-store:REVIEW?PFN={0}", Windows.ApplicationModel.Package.Current.Id.FamilyName)));
                    }));
                }

                // User refuses to rate the app now but maybe later
                if (!string.IsNullOrEmpty(later) && later != NoButton)
                {
                    md.Commands.Add(new UICommand(later, (s) => { SaveSettings(retryCount + 1 >= maxRetry, 0, retryCount + 1); }));
                }

                // User does not want to rate the app at all
                if (!string.IsNullOrEmpty(no) && no != NoButton)
                {
                    md.Commands.Add(new UICommand(no, (s) => { SaveSettings(true, 0, retryCount + 1); }));
                }

                // Prompt the user
                if (yes != NoButton || no != NoButton || later != NoButton)
                {
                    await md.ShowAsync();
                }
            }
        }

        /// <summary>
        /// Sets the settings.
        /// </summary>
        /// <param name="rated">if set to <c>true</c> [rated].</param>
        /// <param name="counter">The counter.</param>
        /// <param name="retry">The retry.</param>
        private static void SaveSettings(bool rated, int counter, int retry)
        {
            //
            // update
            //
            ApplicationData.Current.RoamingSettings.Values[IsRatedKey] = Convert.ToString(rated);
            ApplicationData.Current.RoamingSettings.Values[RatingRetryKey] = Convert.ToString(retry);
            ApplicationData.Current.RoamingSettings.Values[RatingCounterKey] = Convert.ToString(counter);
        }
    }
}
