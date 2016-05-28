using Authenticator_for_Windows.Views.Pages;
using Settings;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private MainPage mainPage;
        private bool loadingSettings;

        public SettingsPage()
        {
            InitializeComponent();

            loadingSettings = true;

            LoadGeneralSettings();

            loadingSettings = false;
        }

        private void LoadGeneralSettings()
        {
            int rememberTime = SettingsManager.Get<int>(Setting.ClipBoardRememberType);

            if (rememberTime <= ClipboardTime.Items.Count)
            {
                ClipboardTime.SelectedIndex = rememberTime;
            }
            
            UseNTP.IsOn = SettingsManager.Get<bool>(Setting.UseNTP);
            NTPTimeout.SelectedIndex = GetIndexOfTimeSpan(SettingsManager.Get<TimeSpan>(Setting.NTPTimeout));

            UseNTP_Toggled(null, null);
        }

        private int GetIndexOfTimeSpan(TimeSpan timeSpan)
        {
            int index = 0;

            if (timeSpan == new TimeSpan(0, 0, 0))
            {
                index = 0;
            }
            else if (timeSpan == new TimeSpan(0, 0, 1))
            {
                index = 1;
            }
            else if (timeSpan == new TimeSpan(0, 0, 2))
            {
                index = 2;
            }
            else if (timeSpan == new TimeSpan(0, 0, 3))
            {
                index = 3;
            }
            else if (timeSpan == new TimeSpan(0, 0, 4))
            {
                index = 4;
            }
            else if (timeSpan == new TimeSpan(0, 0, 5))
            {
                index = 5;
            }

            return index;
        }

        private TimeSpan GetTimeSpanOfIndex(int index)
        {
            return new TimeSpan(0, 0, index + 1);
        }

        private void ClipboardTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingsManager.Save(Setting.ClipBoardRememberType, ClipboardTime.SelectedIndex);
        }

        private void PrivacyDeclaration_Click(object sender, RoutedEventArgs e)
        {
            mainPage.Navigate(typeof(PrivacyDeclaration));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mainPage = (MainPage)e.Parameter;
        }

        private void UseNTP_Toggled(object sender, RoutedEventArgs e)
        {
            NTPTimeout.IsEnabled = UseNTP.IsOn;

            if (!loadingSettings)
            {
                SettingsManager.Save(Setting.UseNTP, UseNTP.IsOn);
            }
        }

        private void NTPTimeout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!loadingSettings)
            {
                SettingsManager.Save(Setting.NTPTimeout, GetTimeSpanOfIndex(NTPTimeout.SelectedIndex));
            }
        }

        private async void OpenWindowsStore_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("ms-windows-store://review/?PFN=" + Package.Current.Id.FamilyName);

            var success = await Launcher.LaunchUriAsync(uri);
        }
    }
}
