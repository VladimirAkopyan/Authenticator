using Microsoft.OneDrive.Sdk;
using Settings;
using Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Authenticator.Views.Settings
{
    public sealed partial class Sync : UserControl
    {
        private PasswordVault vault = new PasswordVault();

        public Sync()
        {
            this.InitializeComponent();
            if (SettingsManager.Get<bool>(Setting.UseCloudSynchronization) && vault.RetrieveAll().Any())
            {
                this.showSyncConfiguration();
            }
            else
            {
                this.hideSyncConfiguration();
            }
        }

        private async void EnableSyncToggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                if (toggleSwitch.IsOn == true)
                {
                    SyncSetupDialog confirmation = new SyncSetupDialog();
                    switch (await confirmation.ShowAsync())
                    {
                        case ContentDialogResult.Primary:
                            SettingsManager.Save(Setting.UseCloudSynchronization, true);
                            // 0 - autoSync, 1 - manual
                            SettingsManager.Save(Setting.WhenToSynchronize, 0); 
                            this.showSyncConfiguration();
                            break;
                        default:
                            toggleSwitch.IsOn = false;
                            break;
                    }
                }
                else
                {
                    this.hideSyncConfiguration();
                    await this.DecommissionSyncWithOnedrive();
                }
            }
        }

        private void AutoSyncToggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            int setAutoSync = toggleSwitch.IsOn ? 0 : 1;
            SettingsManager.Save(Setting.WhenToSynchronize, setAutoSync);
        }

        private async Task DecommissionSyncWithOnedrive()
        {
            IOneDriveClient onedriveClient =
                OneDriveClientExtensions.GetUniversalClient(new[] { "onedrive.appfolder" });

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
