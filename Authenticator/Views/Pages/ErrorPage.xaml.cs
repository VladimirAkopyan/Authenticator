using Domain.Storage;
using Encryption.Exceptions;
using Settings;
using Synchronization.Exceptions;
using System;
using System.Collections.Generic;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ErrorPage : Page
    {
        private Exception exception;

        public ErrorPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            exception = (Exception)e.Parameter;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bool removeCloudSynchronization = false;

            if (exception.GetType() == typeof(StaleException))
            {

            }
            else if (exception.GetType() == typeof(NetworkException))
            {

            }
            else if (exception.GetType() == typeof(InvalidKeyException))
            {
                removeCloudSynchronization = true;
            }
            else if (exception.GetType() == typeof(RemovedSynchronizationException))
            {
                removeCloudSynchronization = true;
            }
            else
            {

            }

            if (removeCloudSynchronization)
            {
                PasswordVault vault = new PasswordVault();
                IReadOnlyList<PasswordCredential> credentials = vault.RetrieveAll();

                foreach (PasswordCredential credential in credentials)
                {
                    vault.Remove(credential);
                }

                AccountStorage.Instance.SetSynchronizer(null);
                SettingsManager.Save(Setting.UseCloudSynchronization, false);
            }

            Type.Text = "Type: " + exception.GetType().ToString();
            TurnedOffCloudSynchronization.Text = TurnedOffCloudSynchronization.Text + ": " + (removeCloudSynchronization ? "YES" : "NO");
        }
    }
}
