using Authenticator_for_Windows.Storage;
using Authenticator_for_Windows.Utilities;
using Authenticator_for_Windows.Views.UserControls;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddPage : Page
    {
        private MainPage mainPage;

        public AddPage()
        {
            InitializeComponent();

            CheckForCamera();
        }

        private async void CheckForCamera()
        {
            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);

            if (devices.Count > 1)
            {
                CommandBar.Visibility = Visibility.Visible;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ScanPage.AccessDenied)
            {
                AccessDeniedDialog dialog = new AccessDeniedDialog();
                await dialog.ShowAsync();
            }
            else
            {
                object[] parameters = (object[])e.Parameter;
                mainPage = (MainPage)parameters[0];

                if (ScanPage.LastScanResult != null)
                {
                    Entry entry = TOTPUtilities.UriToEntry(ScanPage.LastScanResult.Text);

                    ScanPage.LastScanResult = null;

                    if (entry != null)
                    {
                        EntryUsername.Text = entry.Username;
                        EntryCode.Text = entry.Secret;
                        EntryService.Text = entry.Service;

                        if (!string.IsNullOrWhiteSpace(EntryService.Text))
                        {
                            Save_Click(null, null);
                        }
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            MainPage.ClearBanners();
        }

        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            ScanPage.UseCustomOverlay = true;
            ScanPage.CustomOverlay = new CustomOverlay();
            ScanPage.RequestAutoFocus();

            mainPage.Navigate(typeof(ScanPage));
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            EntryBlockPanel.Visibility = Visibility.Collapsed;

            MainPage.ClearBanners();
            EntryBlockPanel.Children.Clear();

            bool valid = true;

            if (string.IsNullOrWhiteSpace(EntryService.Text) || string.IsNullOrWhiteSpace(EntryUsername.Text) || string.IsNullOrWhiteSpace(EntryCode.Text))
            {
                MainPage.AddBanner(new Banner(BannerEmptyFields.BannerType, BannerEmptyFields.BannerText, BannerEmptyFields.Dismissable));

                valid = false;
            }

            if (valid)
            {
                if (!EntryCode.Text.All(char.IsLetterOrDigit))
                {
                    MainPage.AddBanner(new Banner(BannerInvalidCharacters.BannerType, BannerInvalidCharacters.BannerText, BannerInvalidCharacters.Dismissable));

                    valid = false;
                }

                if (valid)
                {
                    Entry entry = new Entry()
                    {
                        Username = EntryUsername.Text,
                        Secret = EntryCode.Text,
                        Service = EntryService.Text
                    };

                    await EntryStorage.Instance.SaveAsync(entry);

                    EntryBlock entryBlock = new EntryBlock(entry, true);

                    MainPage.AddBanner(new Banner(BannerSaved.BannerType, BannerSaved.BannerText, BannerSaved.Dismissable));
                    EntryBlockPanel.Children.Add(entryBlock);

                    EntryBlockPanel.Visibility = Visibility.Visible;

                    EntryService.Text = "";
                    EntryUsername.Text = "";
                    EntryCode.Text = "";
                }
            }
        }
    }
}
