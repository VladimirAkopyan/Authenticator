using Domain.Utilities;
using Microsoft.OneDrive.Sdk;
using Synchronization;
using System.Collections.Generic;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using System.Text.RegularExpressions;
using Authenticator.Views.UserControls;
using Windows.ApplicationModel.Resources;
using Domain.Storage;
using Encryption;
using Encryption.Exceptions;
using Synchronization.Exceptions;
using Windows.UI.Popups;
using System;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetupSynchronizationPage : Page
    {
        private const string RESOURCE_NAME = "EncryptionKey";
        private const string USERNAME_NAME = "Encryption Key";

        private string userKey;
        private PasswordVault vault;
        private ISynchronizer synchronizer;
        private MainPage mainPage;

        public SetupSynchronizationPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            object[] parameters = (object[])e.Parameter;

            synchronizer = (ISynchronizer)parameters[0];
            mainPage = (MainPage)parameters[1];
            vault = new PasswordVault();

            AccountStorage.Instance.SetSynchronizer(synchronizer);

            if (synchronizer.IsInitialSetup)
            {
                if (vault.RetrieveAll().Any())
                {
                    IReadOnlyList<PasswordCredential> credentials = vault.FindAllByResource(RESOURCE_NAME);

                    foreach (PasswordCredential credential in credentials)
                    {
                        vault.Remove(credential);
                    }
                }

                userKey = KeyGenerator.GetRandomKey();

                UserKey.Text = userKey;

                vault.Add(new PasswordCredential(RESOURCE_NAME, USERNAME_NAME, userKey));

                InitialSetupPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ConnectPanel.Visibility = Visibility.Visible;
            }
        }

        private async void Continue_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                MainPage.ShowLoader(ResourceLoader.GetForCurrentView().GetString("SynchronizingAccountsWithCloud"));

                IOneDriveClient oneDriveClient = OneDriveClientExtensions.GetUniversalClient(new[] { "onedrive.appfolder" });
                AccountSession session = await oneDriveClient.AuthenticateAsync();

                IEncrypter encrypter = new AESEncrypter();

                if (session.AccountType == AccountType.MicrosoftAccount)
                {
                    synchronizer.SetEncrypter(encrypter, userKey);
                    await AccountStorage.Instance.Synchronize();

                    MainPage.HideLoader();

                    Frame.Navigate(typeof(SetupSynchronizationFinishedPage), mainPage);
                }
            }
            catch (OneDriveException ex)
            {
                MessageDialog dialog = GetOneDriveErrorMessageDialog(ex);
                await dialog.ShowAsync();
            }
        }

        private MessageDialog GetOneDriveErrorMessageDialog(OneDriveException exception)
        {
            string error = ResourceLoader.GetForCurrentView().GetString("OneDriveError" + exception.Error.Code);

            if (string.IsNullOrWhiteSpace(error))
            {
                error = string.Format("{0} ({1})", ResourceLoader.GetForCurrentView().GetString("OneDriveErrorUnknown"), exception.Error.Code);

                if (exception.Error.InnerError != null)
                {
                    error += string.Format(" ({1})", exception.Error.InnerError.Code);
                }
            }

            string message = string.Format("{0}:{1}{2}", ResourceLoader.GetForCurrentView().GetString("CloudSynchronizationOneDriveErrorMessage"), Environment.NewLine, error);
            MessageDialog dialog = new MessageDialog(message, ResourceLoader.GetForCurrentView().GetString("CloudSynchronizationOneDriveErrorTitle"));
            
            return dialog;
        }

        private void UserKeyToValidate_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonConnect.IsEnabled = UserKeyToValidate.ContainsValidUserKey;
        }

        private async void ButtonConnect_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (UserKeyToValidate.Text.Length == 23)
            {
                MainPage.ShowLoader(ResourceLoader.GetForCurrentView().GetString("CheckingKey"));

                IEncrypter encrypter = new AESEncrypter();
                synchronizer.SetEncrypter(encrypter, UserKeyToValidate.Text);

                bool result = false;

                try
                {
                    result = await synchronizer.DecryptWithKey(UserKeyToValidate.Text);

                    if (result)
                    {
                        MainPage.ShowLoader(ResourceLoader.GetForCurrentView().GetString("SynchronizingAccountsWithCloud"));

                        try
                        {
                            await AccountStorage.Instance.Synchronize();

                            vault.Add(new PasswordCredential(RESOURCE_NAME, USERNAME_NAME, UserKeyToValidate.Text));

                            Frame.Navigate(typeof(SetupSynchronizationFinishedPage), mainPage);
                        }
                        catch (OneDriveException ex)
                        {
                            MessageDialog dialog = GetOneDriveErrorMessageDialog(ex);
                            await dialog.ShowAsync();
                        }
                        finally
                        {
                            MainPage.HideLoader();
                        }
                    }
                    else
                    {
                        MainPage.HideLoader();

                        MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("EnteredKeyIncorrect"), true));
                    }
                }
                catch (NetworkException)
                {
                    MainPage.HideLoader();

                    MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerUnableToValidateKey"), true));
                }
            }
        }
    }
}