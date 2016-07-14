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
using Windows.Security.Credentials;
using Authenticator_for_Windows.Views.UserControls;
using Windows.UI.Xaml.Controls.Primitives;
using Domain.Storage;
using Synchronization.Exceptions;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.ApplicationModel.Resources;

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
            LoadSynchronizationSettings();

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

        private void LoadSynchronizationSettings()
        {
            WhenToSynchronize.SelectedIndex = SettingsManager.Get<int>(Setting.WhenToSynchronize);
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

                if (!ex.IsMatch(OneDriveErrorCode.Unauthenticated.ToString()) && !ex.IsMatch(OneDriveErrorCode.AccessDenied.ToString()) && !ex.IsMatch(OneDriveErrorCode.AuthenticationCancelled.ToString()) && !ex.IsMatch(OneDriveErrorCode.AuthenticationFailure.ToString()))
                {
                    MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("UnknownErrorWhileAuthenticating"), true));
                }
            }
        }
        
        private async Task DisableSynchronization()
        {
            IReadOnlyList<PasswordCredential> credentials = vault.RetrieveAll();

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

        private async void ButtonRemoveCloudSynchronization_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("RemoveSynchronizationMessage"), ResourceLoader.GetForCurrentView().GetString("RemoveSynchronizationTitle"));
            dialog.Commands.Add(new UICommand() { Label = ResourceLoader.GetForCurrentView().GetString("Delete"), Id = 0 });
            dialog.Commands.Add(new UICommand() { Label = ResourceLoader.GetForCurrentView().GetString("Cancel"), Id = 1 });

            dialog.CancelCommandIndex = 1;
            dialog.DefaultCommandIndex = 1;

            IUICommand selectedCommand = await dialog.ShowAsync();

            if ((int)selectedCommand.Id == 0)
            {
                Banner banner = new Banner();
                bool result = false;

                try
                {
                    MainPage.ShowLoader(ResourceLoader.GetForCurrentView().GetString("AccountsAreBeingRemovedFromCloud"));

                    ISynchronizer synchronizer = new OneDriveSynchronizer(oneDriveClient);
                    result = await synchronizer.Remove();
                }
                catch (NetworkException)
                {
                    result = false;
                }                

                if (result)
                {
                    banner.BannerText = ResourceLoader.GetForCurrentView().GetString("SynchronizationRemoved");
                    banner.BannerType = BannerType.Success;
                    banner.Dismissable = true;
                }
                else
                {
                    banner.BannerText = ResourceLoader.GetForCurrentView().GetString("SynchronizationRemovedError");
                    banner.BannerType = BannerType.Danger;
                    banner.Dismissable = true;
                }

                MainPage.AddBanner(banner);

                await DisableSynchronization();
                ShowInformation();

                MainPage.HideLoader();
            }
        }

        private async void ButtonTurnOffSynchronization_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("TurnOffSynchronizationMessage"), ResourceLoader.GetForCurrentView().GetString("TurnOffSynchronizationTitle"));
            dialog.Commands.Add(new UICommand() { Label = ResourceLoader.GetForCurrentView().GetString("TurnOff"), Id = 0 });
            dialog.Commands.Add(new UICommand() { Label = ResourceLoader.GetForCurrentView().GetString("Cancel"), Id = 1 });

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

        private void WhenToSynchronize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!loadingSettings)
            {
                SettingsManager.Save(Setting.WhenToSynchronize, WhenToSynchronize.SelectedIndex);
            }
        }
    }
}
