using Domain.Storage;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Domain.Utilities;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel.Resources;
using Domain;
using Authenticator_for_Windows.Views.UserControls;
using Domain.Exceptions;
using Synchronization.Exceptions;
using Settings;

namespace Authenticator_for_Windows.Views.Pages
{
    public sealed partial class AddPage : Page
    {
        private static MainPage mainPage;
        private AccountBlock accountBlock;

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
                Scan.Visibility = Visibility.Visible;
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
                try
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
                                Save_Tapped(null, null);
                            }
                        }
                        else
                        {
                            MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerInvalidCode"), true));
                        }
                    }
                }
                catch
                {
                    MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerInvalidCode"), true));
                }
            }

            mainPage.SetTitle();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            MainPage.ClearBanners();

            AccountStorage.Instance.SynchronizationCompleted -= SynchronizationCompleted;
        }

        private void SynchronizationCompleted(object sender, Synchronization.SynchronizationResult e)
        {
            Synchronize.StopAnimationAndEnable();
            Synchronize.IsEnabled = false;

            SetButtonState(true);
        }

        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            ScanPage.UseCustomOverlay = true;
            ScanPage.CustomOverlay = new CustomOverlay();
            ScanPage.RequestAutoFocus();

            mainPage.Navigate(typeof(ScanPage));
        }

        private void SetButtonState(bool enabled)
        {
            Scan.IsEnabled = enabled;

            Save.IsLoading = !enabled;
        }

        private void AccountBlock_CopyRequested(object sender, EventArgs e)
        {
            CodeCopiedNotification.Animate();
        }

        private void OpenFlyout(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void TimeProgressBar_TimeElapsed(object sender, EventArgs e)
        {
            if (accountBlock != null)
            {
                accountBlock.Update();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsManager.Get<bool>(Setting.UseCloudSynchronization) && AccountStorage.Instance.HasSynchronizer)
            {
                Synchronize.Visibility = Visibility.Visible;

                AccountStorage.Instance.SynchronizationCompleted += SynchronizationCompleted;

                if (AccountStorage.Instance.IsSynchronizing)
                {
                    Synchronize.StartAnimationAndDisable();

                    SetButtonState(false);
                }
            }
            else
            {
                if (Scan.Visibility == Visibility.Collapsed)
                {
                    CommandBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void Save_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Synchronize.StartAnimationAndDisable();
            SetButtonState(false);

            AccountBlockPanel.Visibility = Visibility.Collapsed;

            MainPage.ClearBanners();
            AccountBlockPanel.Children.Clear();

            bool valid = true;

            if (string.IsNullOrWhiteSpace(AccountService.Text) || string.IsNullOrWhiteSpace(AccountUsername.Text) || string.IsNullOrWhiteSpace(AccountCode.Text))
            {
                MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerEmptyFields"), true));

                valid = false;
            }

            if (valid)
            {
                try
                {
                    if (!AccountCode.Text.All(char.IsLetterOrDigit))
                    {
                        MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerInvalidCharacters"), true));

                        valid = false;
                    }

                    if (valid)
                    {
                        OTP otp = new OTP(AccountCode.Text);

                        Account account = new Account(AccountUsername.Text, AccountCode.Text, AccountService.Text);

                        await AccountStorage.Instance.SaveAsync(account);

                        accountBlock = new AccountBlock(account, true, mainPage);
                        accountBlock.CopyRequested += AccountBlock_CopyRequested;

                        MainPage.AddBanner(new Banner(BannerType.Success, ResourceLoader.GetForCurrentView().GetString("BannerSaved"), true));
                        AccountBlockPanel.Children.Add(accountBlock);

                        AccountBlockPanel.Visibility = Visibility.Visible;

                        AccountService.Text = "";
                        AccountUsername.Text = "";
                        AccountCode.Text = "";
                    }
                }
                catch (ArgumentException)
                {
                    MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerInvalidCharacters"), true));
                }
                catch (DuplicateAccountException)
                {
                    MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerDuplicateAccount"), true));
                }
                catch (NetworkException)
                {
                    MainPage.AddBanner(new Banner(BannerType.Danger, "Het lijkt erop dat u geen werkende internetverbinding heeft. Voor cloudsynchronisatie is een werkende internetverbinding vereist.", true));
                }
                catch (Exception)
                {
                    MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerUnknownError"), true));
                }
            }

            SetButtonState(true);
            Synchronize.StopAnimationAndEnable();
            Synchronize.IsEnabled = false;
        }
    }
}
