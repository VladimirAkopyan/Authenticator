using Domain.Storage;
using Encryption.Exceptions;
using Settings;
using Synchronization.Exceptions;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
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

                title = ResourceLoader.GetForCurrentView().GetString("InvalidPersonalKey");
                message = ResourceLoader.GetForCurrentView().GetString("PersonalKeyChanged");
            }
            else if (exception.GetType() == typeof(RemovedSynchronizationException))
            {
                removeCloudSynchronization = true;
                title = ResourceLoader.GetForCurrentView().GetString("RemovedCloudSynchronization");
                message = ResourceLoader.GetForCurrentView().GetString("RemovedCloudSynchronizationFromOtherDevice");
            }
            else
            {
                title = ResourceLoader.GetForCurrentView().GetString("UnknownError");
                message = ResourceLoader.GetForCurrentView().GetString("UnknownErrorDuringSynchronize");
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
