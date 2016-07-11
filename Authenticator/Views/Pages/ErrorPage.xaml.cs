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

            MainPage.Instance.ClearBackStack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bool removeCloudSynchronization = false;
            string title = "";
            string message = "";

            if (exception.GetType() == typeof(InvalidKeyException))
            {
                removeCloudSynchronization = true;
                title = "Ongeldige persoonlijke sleutel";
                message = "Uw persoonlijke sleutel is veranderd vanaf een ander apparaat. Omdat deze persoonlijke sleutel benodigd is voor het ontsleutelen van uw accounts in de cloud, is cloudsynchronisatie nu uitgeschakeld.";
            }
            else if (exception.GetType() == typeof(RemovedSynchronizationException))
            {
                removeCloudSynchronization = true;
                title = "Cloudsynchronisatie verwijderd";
                message = "Cloudsynchronisatie is vanaf een ander apparaat verwijderd, daarom is cloudsynchronisatie op dit apparaat nu uitgeschakeld.";
            }
            else
            {
                title = "Onbekende fout";
                message = "Er is een onbekende fout opgetreden bij het synchroniseren van uw accounts met de cloud.";
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

            ErrorTitle.Text = title;
            ErrorMessage.Text = message;
        }

        private void ViewCodes_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToAccountsAndClearBackStack();
        }
    }
}
