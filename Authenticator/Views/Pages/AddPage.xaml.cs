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
using Authenticator.Views.UserControls;
using Domain.Exceptions;
using Synchronization.Exceptions;
using Settings;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using ZXing;
using ZXing.Common;
using Windows.Storage.Streams;
using ZXing.Mobile;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;
using Windows.Media.Capture;
using System.Collections.Generic;

namespace Authenticator.Views.Pages
{
    public sealed partial class AddPage : Page
    {
        private static MainPage mainPage;
        private AccountBlock accountBlock;
        private Account dragAndDropAccount;
        private static Account scannedAccount;
        private static bool didScan;
        private MobileBarcodeScanner scanner;

        public AddPage()
        {
            InitializeComponent();

            try
            {
                scanner = new MobileBarcodeScanner(Dispatcher);
                scanner.Dispatcher = Dispatcher;
            }
            catch (Exception e)
            {

            }
        }

        private async Task CheckForCamera()
        {
            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);

            if (devices.Count > 1)
            {
                Scan.Visibility = Visibility.Visible;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                object[] parameters = (object[])e.Parameter;
                mainPage = (MainPage)parameters[0];

                mainPage.SetTitle();

                if (didScan)
                {
                    try
                    {
                        if (scannedAccount != null)
                        {
                            AccountUsername.Text = scannedAccount.Username;
                            AccountCode.Text = scannedAccount.Secret;
                            AccountService.Text = scannedAccount.Service;

                            if (!string.IsNullOrWhiteSpace(AccountUsername.Text) && !string.IsNullOrWhiteSpace(AccountCode.Text) && !string.IsNullOrWhiteSpace(AccountService.Text))
                            {
                                Save_Tapped(null, null);
                            }
                        }
                        else
                        {
                            MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerInvalidCode"), true));
                        }
                    }
                    catch
                    {
                        MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerInvalidCode"), true));
                    }
                }

                didScan = false;
            }
            catch (Exception)
            {

            }
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

        private async void Scan_Click(object sender, RoutedEventArgs e)
        {
            scanner.UseCustomOverlay = true;
            scanner.CustomOverlay = new CustomOverlay();
            scanner.AutoFocus();
            scanner.RootFrame = Frame;

            try
            {
                MediaCapture capture = new MediaCapture();
                await capture.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Video
                });

                mainPage.SetTitle(ResourceLoader.GetForCurrentView().GetString("ScanTitle"));

                await scanner.Scan(new MobileBarcodeScanningOptions()
                {
                    PossibleFormats = new List<BarcodeFormat>()
                    {
                        BarcodeFormat.QR_CODE
                    }
                }).ContinueWith(t =>
                {
                    HandleScanResult(t.Result);

                    didScan = true;
                });
            }
            catch (UnauthorizedAccessException)
            {
                AccessDeniedDialog dialog = new AccessDeniedDialog();
                await dialog.ShowAsync();
            }
        }

        private void HandleScanResult(Result result)
        {
            if (result != null)
            {
                scannedAccount = TOTPUtilities.UriToAccount(result.Text);
            }
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckForCamera();

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
                    string code = new string(Array.FindAll(AccountCode.Text.ToCharArray(), (c => (char.IsLetterOrDigit(c)))));

                    if (!code.All(char.IsLetterOrDigit))
                    {
                        MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerInvalidCharacters"), true));

                        valid = false;
                    }

                    if (valid)
                    {
                        OTP otp = new OTP(code);

                        Account account = new Account(AccountUsername.Text, code, AccountService.Text);

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
                    MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("NoInternetConnection"), true));
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

        private async void Grid_DragEnter(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;

            try
            {
                var files = await e.DataView.GetStorageItemsAsync();

                if (files.Count == 1)
                {
                    StorageFile file = files.FirstOrDefault() as StorageFile;

                    if (file != null)
                    {
                        IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);

                        Result result = await Decode(stream);

                        if (result != null)
                        {
                            dragAndDropAccount = TOTPUtilities.UriToAccount(result.Text);

                            if (dragAndDropAccount != null)
                            {
                                BitmapImage bi = new BitmapImage();
                                bi.SetSource(stream);

                                QRCodeImage.Source = bi;

                                VisualStateManager.GoToState(this, ShowDrop.Name, true);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Could not read dragged items. Since this is definitely not a QR code, ignore it.
            }
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            VisualStateManager.GoToState(this, HideDrop.Name, true);

            if (dragAndDropAccount != null)
            {
                AccountUsername.Text = !string.IsNullOrWhiteSpace(dragAndDropAccount.Username) ? dragAndDropAccount.Username : "";
                AccountCode.Text = !string.IsNullOrWhiteSpace(dragAndDropAccount.Secret) ? dragAndDropAccount.Secret : "";
                AccountService.Text = !string.IsNullOrWhiteSpace(dragAndDropAccount.Service) ? dragAndDropAccount.Service : "";

                if (!string.IsNullOrWhiteSpace(AccountUsername.Text) && !string.IsNullOrWhiteSpace(AccountCode.Text) && !string.IsNullOrWhiteSpace(AccountService.Text))
                {
                    Save_Tapped(null, null);
                }
            }
        }

        private async Task<Result> Decode(IRandomAccessStream stream)
        {
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync();

            LuminanceSource luminanceSource = new SoftwareBitmapLuminanceSource(bitmap);
            Binarizer binarizer = new HybridBinarizer(luminanceSource);
            BinaryBitmap binaryBitmap = new BinaryBitmap(binarizer);
            MultiFormatReader reader = new MultiFormatReader();

            return reader.decode(binaryBitmap);
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            VisualStateManager.GoToState(this, HideDrop.Name, true);
        }
    }
}
