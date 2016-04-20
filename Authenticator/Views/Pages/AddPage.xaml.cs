using Domain.Storage;
using Domain.Extensions;
using Domain.Views.UserControls;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Domain.Utilities;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Domain.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddPage : Page
    {
        private static MainPage mainPage;

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
                    Account account = TOTPUtilities.UriToAccount(ScanPage.LastScanResult.Text);

                    ScanPage.LastScanResult = null;

                    if (account != null)
                    {
                        AccountUsername.Text = account.Username;
                        AccountCode.Text = account.Secret;
                        AccountService.Text = account.Service;

                        if (!string.IsNullOrWhiteSpace(AccountService.Text))
                        {
                            Save_Click(null, null);
                        }
                    }
                    else
                    {
                        MainPage.AddBanner(new Banner(BannerInvalidCode.BannerType, BannerInvalidCode.BannerText, BannerInvalidCode.Dismissable));
                    }
                }
            }

            mainPage.SetTitle();
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
            AccountBlockPanel.Visibility = Visibility.Collapsed;

            MainPage.ClearBanners();
            AccountBlockPanel.Children.Clear();

            bool valid = true;

            if (string.IsNullOrWhiteSpace(AccountService.Text) || string.IsNullOrWhiteSpace(AccountUsername.Text) || string.IsNullOrWhiteSpace(AccountCode.Text))
            {
                MainPage.AddBanner(new Banner(BannerEmptyFields.BannerType, BannerEmptyFields.BannerText, BannerEmptyFields.Dismissable));

                valid = false;
            }

            if (valid)
            {
                try
                {
                    if (!AccountCode.Text.All(char.IsLetterOrDigit))
                    {
                        MainPage.AddBanner(new Banner(BannerInvalidCharacters.BannerType, BannerInvalidCharacters.BannerText, BannerInvalidCharacters.Dismissable));

                        valid = false;
                    }

                    if (valid)
                    {
                        OTP otp = new OTP(AccountCode.Text);
                        
                        Account account = new Account()
                        {
                            Username = AccountUsername.Text,
                            Secret = AccountCode.Text,
                            Service = AccountService.Text
                        };

                        await AccountStorage.Instance.SaveAsync(account);

                        AccountBlock accountBlock = new AccountBlock(account, true);

                        MainPage.AddBanner(new Banner(BannerSaved.BannerType, BannerSaved.BannerText, BannerSaved.Dismissable));
                        AccountBlockPanel.Children.Add(accountBlock);

                        AccountBlockPanel.Visibility = Visibility.Visible;

                        AccountService.Text = "";
                        AccountUsername.Text = "";
                        AccountCode.Text = "";
                    }
                }
                catch (ArgumentException)
                {
                    MainPage.AddBanner(new Banner(BannerInvalidCharacters.BannerType, BannerInvalidCharacters.BannerText, BannerInvalidCharacters.Dismissable));
                }
                catch (Exception)
                {
                    MainPage.AddBanner(new Banner(BannerUnknownError.BannerType, BannerUnknownError.BannerText, BannerUnknownError.Dismissable));
                }
            }
        }
    }
}
