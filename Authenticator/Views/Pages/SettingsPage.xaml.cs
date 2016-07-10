﻿using Microsoft.OneDrive.Sdk;
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
using Windows.Security.Credentials;
using Authenticator_for_Windows.Views.UserControls;
using Windows.UI.Xaml.Controls.Primitives;
using Domain.Storage;
using Synchronization.Exceptions;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Authenticator_for_Windows.Views.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private IOneDriveClient oneDriveClient;
        private MainPage mainPage;
        private PasswordVault vault;
        private bool loadingSettings;

        private const string RESOURCE_NAME = "EncryptionKey";

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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            oneDriveClient = OneDriveClientExtensions.GetUniversalClient(new[] { "onedrive.appfolder" });

            vault = new PasswordVault();

            ShowInformation();
        }

        private async void ButtonTurnOnSynchronization_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                ButtonTurnOnSynchronization.IsLoading = true;

                AccountSession session = await oneDriveClient.AuthenticateAsync();

                if (session.AccountType == AccountType.MicrosoftAccount)
                {
                    ISynchronizer synchronizer = new OneDriveSynchronizer(oneDriveClient);
                    await synchronizer.Setup();
                    
                    mainPage.Navigate(typeof(SetupSynchronizationPage), new object[] { synchronizer, mainPage });
                }
            }
            catch (OneDriveException ex)
            {
                ButtonTurnOnSynchronization.IsLoading = false;

                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
        
        private async Task DisableSynchronization()
        {
            IReadOnlyList<PasswordCredential> credentials = vault.FindAllByResource(RESOURCE_NAME);

            foreach (PasswordCredential credential in credentials)
            {
                vault.Remove(credential);
            }

            await oneDriveClient.SignOutAsync();
            SettingsManager.Save(Setting.UseCloudSynchronization, false);
        }

        private void ShowInformation()
        {
            if (SettingsManager.Get<bool>(Setting.UseCloudSynchronization) && vault.RetrieveAll().Any())
            {
                SynchronizationOff.Visibility = Visibility.Collapsed;
                SynchronizationOn.Visibility = Visibility.Visible;
            }
            else
            {
                SynchronizationOn.Visibility = Visibility.Collapsed;
                SynchronizationOff.Visibility = Visibility.Visible;
            }
        }

        private async void ShowUserKey_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            IReadOnlyList<PasswordCredential> credentials = vault.FindAllByResource(RESOURCE_NAME);

            if (credentials.Any())
            {
                credentials[0].RetrievePassword();

                ShowUserKeyDialog dialog = new ShowUserKeyDialog(credentials[0].Password);
                await dialog.ShowAsync();
            }
            else
            {
                ShowInformation();
            }
        }

        private async void ButtonRemoveSynchronization_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("Weet u zeker dat u cloudsynchronisatie wilt uitschakelen ? Dit geldt voor alle apparaten waarop u cloudsynchronisatie heeft ingeschakeld.", "Cloudsynchronisatie verwijderen");
            dialog.Commands.Add(new UICommand() { Label = "Verwijderen", Id = 0 });
            dialog.Commands.Add(new UICommand() { Label = "Annuleren", Id = 1 });

            dialog.CancelCommandIndex = 1;
            dialog.DefaultCommandIndex = 1;

            IUICommand selectedCommand = await dialog.ShowAsync();

            if ((int)selectedCommand.Id == 0)
            {
                try
                {
                    ISynchronizer synchronizer = new OneDriveSynchronizer(oneDriveClient);
                    bool result = await synchronizer.Remove();

                    Banner banner = new Banner();

                    if (result)
                    {
                        banner.BannerText = "Cloudsynchronisatie is succesvol verwijderd.";
                        banner.BannerType = BannerType.Success;
                        banner.Dismissable = true;
                    }
                    else
                    {
                        banner.BannerText = "Cloudsynchronisatie is niet verwijderd. Probeer het later opnieuw.";
                        banner.BannerType = BannerType.Danger;
                        banner.Dismissable = true;
                    }
                }
                catch (NetworkException)
                {
                    // TODO: Implement this exception.
                }

                await DisableSynchronization();
                ShowInformation();
            }
        }

        private async void ButtonTurnOffSynchronization_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("Weet u zeker dat u cloudsynchronisatie wilt uitschakelen?", "Cloudsynchronisatie uitschakelen");
            dialog.Commands.Add(new UICommand() { Label = "Uitschakelen", Id = 0 });
            dialog.Commands.Add(new UICommand() { Label = "Annuleren", Id = 1 });

            dialog.CancelCommandIndex = 1;
            dialog.DefaultCommandIndex = 1;

            IUICommand selectedCommand = await dialog.ShowAsync();

            if ((int)selectedCommand.Id == 0)
            {
                await DisableSynchronization();
                
                AccountStorage.Instance.SetSynchronizer(null);

                ShowInformation();
            }
        }
    }
}
