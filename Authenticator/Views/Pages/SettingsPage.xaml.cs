using Microsoft.OneDrive.Sdk;
using Settings;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Domain.Utilities;
using Synchronization;
using System.IO;

namespace Authenticator_for_Windows.Views.Pages
{
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                IOneDriveClient oneDriveClient = OneDriveClientExtensions.GetUniversalClient(new[] { "onedrive.appfolder" });
                AccountSession session = await oneDriveClient.AuthenticateAsync();

                if (session.AccountType == AccountType.MicrosoftAccount)
                {
                    ISynchronizer synchronizer = new OneDriveSynchronizer(oneDriveClient);
                    await synchronizer.Setup();
                    //using (var stream = GenerateStreamFromString("testje"))
                    //{
                    //    var item = await oneDriveClient.Drive.Special.AppRoot
                    //      .ItemWithPath("key.txt")
                    //      .Content.Request()
                    //      .PutAsync<Item>(stream);
                    //}
                    
                    await synchronizer.Synchronize();
                }
            }
            catch (OneDriveException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}
