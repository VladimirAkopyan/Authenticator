using Microsoft.OneDrive.Sdk;
using Settings;
using Synchronization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Authenticator.Views.Settings
{
    public sealed partial class CloudSyncSettings : UserControl
    {
        private IOneDriveClient onedriveClient;
        private PasswordVault vault;

        public CloudSyncSettings()
        {
            this.InitializeComponent();
            onedriveClient = OneDriveClientExtensions.GetUniversalClient(new[] { "onedrive.appfolder" });
            vault = new PasswordVault();

            if (SettingsManager.Get<bool>(Setting.UseCloudSynchronization) && vault.RetrieveAll().Any())
            {
                this.showSyncConfiguration();
            }
            else
            {
                this.hideSyncConfiguration();
            }
        }

        private async void EnableSyncToggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;

            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    this.showSyncConfiguration();
                    try
                    {
                        AccountSession session = await onedriveClient.AuthenticateAsync();
                        if (session.AccountType == AccountType.MicrosoftAccount)
                        {
                            ISynchronizer synchronizer = new OneDriveSynchronizer(onedriveClient);
                            await synchronizer.Setup();
                            //mainPage.Navigate(typeof(SetupSynchronizationPage), new object[] { synchronizer, mainPage });
                        }
                    }
                    catch (OneDriveException ex)
                    {
                        if (!ex.IsMatch(OneDriveErrorCode.Unauthenticated.ToString()) && !ex.IsMatch(OneDriveErrorCode.AccessDenied.ToString()) && !ex.IsMatch(OneDriveErrorCode.AuthenticationCancelled.ToString()) && !ex.IsMatch(OneDriveErrorCode.AuthenticationFailure.ToString()))
                        {
                            //MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("UnknownErrorWhileAuthenticating"), true));
                        }
                    }
                }
                else
                {
                    await this.DecommissionSyncWithOnedrive();
                    this.hideSyncConfiguration();
                }
            }
        }

        private async void AutoSyncToggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
        }

        private async Task DecommissionSyncWithOnedrive()
        {
            IReadOnlyList<PasswordCredential> credentials = vault.RetrieveAll();
            foreach (PasswordCredential credential in credentials)
            {
                vault.Remove(credential);
            }
            await onedriveClient.SignOutAsync();
            SettingsManager.Save(Setting.UseCloudSynchronization, false);
        }

        private void showSyncConfiguration()
        {
            this.warningTextBlock.Visibility = Visibility.Visible;
            this.autoSyncStackPanel.Visibility = Visibility.Visible;

            int syncFrequencySetting = SettingsManager.Get<int>(Setting.WhenToSynchronize);
            autoSyncToggle.IsOn = syncFrequencySetting == 0;
        }

        private void hideSyncConfiguration()
        {
            this.warningTextBlock.Visibility = Visibility.Collapsed;
            this.autoSyncStackPanel.Visibility = Visibility.Collapsed;
        }
    }
}
