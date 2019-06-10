using Domain.Storage;
using Domain.Utilities;
using Microsoft.OneDrive.Sdk;
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
    public sealed partial class SyncSetupDialog : ContentDialog
    {
        private readonly string encryptionKey = KeyGenerator.GetRandomKey();
        private readonly PasswordVault vault = new PasswordVault();

        private readonly IOneDriveClient onedriveClient = 
            OneDriveClientExtensions.GetUniversalClient(new[] { "onedrive.appfolder" });

        private readonly ISynchronizer synchronizer;

        public SyncSetupDialog()
        {
            this.InitializeComponent();

            this.synchronizer = new OneDriveSynchronizer(this.onedriveClient);
            this.Opened += this.DialogOpened;
            this.Closing += this.DialogClosing;
        }

        async void DialogOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            await this.setupSync();
        }

        void DialogClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {

        }

        private void ContinueButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void RetryOnedriveAuth(object sender, RoutedEventArgs e)
        {
            await this.setupSync();

        }

        private async Task setupSync()
        {
            this.progressRing.IsActive = true;

            try
            {
                AccountSession session = await this.onedriveClient.AuthenticateAsync();
                if (session.AccountType == AccountType.MicrosoftAccount)
                {
                    await this.synchronizer.Setup();
                    AccountStorage.Instance.SetSynchronizer(this.synchronizer);

                    // Remove all existing encryption keys, if any
                    // Then add the new encryption key to secure storage
                    if (this.vault.RetrieveAll().Any())
                    {
                        IReadOnlyList<PasswordCredential> credentials = this.vault.FindAllByResource("EncryptionKey");
                        foreach (PasswordCredential credential in credentials)
                        {
                            this.vault.Remove(credential);
                        }
                    }
                    this.vault.Add(new PasswordCredential("EncryptionKey", "Encryption Key", this.encryptionKey));


                    this.showSyncInfo();
                    this.IsPrimaryButtonEnabled = true;
                    this.DefaultButton = ContentDialogButton.Primary;
                }
            }
            catch (OneDriveException ex)
            {
                if (!ex.IsMatch(OneDriveErrorCode.Unauthenticated.ToString()) && !ex.IsMatch(OneDriveErrorCode.AccessDenied.ToString()) && !ex.IsMatch(OneDriveErrorCode.AuthenticationCancelled.ToString()) && !ex.IsMatch(OneDriveErrorCode.AuthenticationFailure.ToString()))
                {
                    this.onedriveErrorMessage.Text = "Oops! We are having trouble connecting to OneDrive. Please try again.";
                    this.showOnedriveError();
                }
                else
                {
                    this.showOnedriveError();
                }
            }

            this.progressRing.IsActive = false;
        }

        private void showSyncInfo()
        {
            this.progressRing.Visibility = Visibility.Collapsed;
            this.onedriveErrorPanel.Visibility = Visibility.Collapsed;
            this.syncInfoPanel.Visibility = Visibility.Visible;
        }

        private void showOnedriveError()
        {
            this.progressRing.Visibility = Visibility.Collapsed;
            this.onedriveErrorPanel.Visibility = Visibility.Visible;
            this.syncInfoPanel.Visibility = Visibility.Collapsed;
        }
    }
}
