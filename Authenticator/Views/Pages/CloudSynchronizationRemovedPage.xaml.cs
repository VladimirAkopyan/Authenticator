using Domain.Storage;
using Settings;
using System.Collections.Generic;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CloudSynchronizationRemovedPage : Page
    {
        public CloudSynchronizationRemovedPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
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
    }
}
