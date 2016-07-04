using Domain.Utilities;
using Microsoft.OneDrive.Sdk;
using Synchronization;
using System.Collections.Generic;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetupSynchronizationPage : Page
    {
        private const string RESOURCE_NAME = "EncryptionKey";
        private const string USERNAME_NAME = "Encryption Key";

        private string userKey;

        public SetupSynchronizationPage()
        {
            InitializeComponent();

            PasswordVault vault = new PasswordVault();

            IReadOnlyList<PasswordCredential> credentials = vault.FindAllByResource(RESOURCE_NAME);

            foreach (PasswordCredential credential in credentials)
            {
                vault.Remove(credential);
            }

            userKey = KeyGenerator.GetRandomKey();

            UserKey.Text = userKey;

            vault.Add(new PasswordCredential(RESOURCE_NAME, USERNAME_NAME, userKey));
        }

        private async void Continue_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, ShowUploading.Name, true);


            IOneDriveClient oneDriveClient = OneDriveClientExtensions.GetUniversalClient(new[] { "onedrive.appfolder" });
            AccountSession session = await oneDriveClient.AuthenticateAsync();

            if (session.AccountType == AccountType.MicrosoftAccount)
            {
                ISynchronizer synchronizer = new OneDriveSynchronizer(oneDriveClient);
                synchronizer.SetUserKey(userKey);
                await synchronizer.Synchronize();

                Frame.Navigate(typeof(SetupSynchronizationFinishedPage));
            }
        }
    }
}