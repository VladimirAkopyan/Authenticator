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
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using ZXing;
using System.IO;
using ZXing.QrCode;
using ZXing.Common;
using Windows.Storage.Streams;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using ZXing.Mobile;
using Windows.Graphics.Imaging;

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
                                Save_Click(null, null);
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

                        accountBlock = new AccountBlock(account, true);
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
                catch (Exception)
                {
                    MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("BannerUnknownError"), true));
                }
            }
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

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            var files = await e.DataView.GetStorageItemsAsync();

            if (files.Count == 1)
            {
                StorageFile file = files.FirstOrDefault() as StorageFile;

                if (file != null)
                {
                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                    BitmapImage bi = new BitmapImage();
                    bi.SetSource(stream);

                    image.Source = bi;

                    var datareader = new DataReader(stream.GetInputStreamAt(0));
                    var bytes = new byte[stream.Size];
                    await datareader.LoadAsync((uint)stream.Size);
                    datareader.ReadBytes(bytes);

                    QRCodeReader reader = new QRCodeReader();

                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    SoftwareBitmap sfbmp = await decoder.GetSoftwareBitmapAsync();

                    RGBLuminanceSource rgb = new RGBLuminanceSource(bytes, 200, 200);
                    
                    SoftwareBitmap s = new SoftwareBitmap(BitmapPixelFormat.Bgra8, sfbmp.PixelWidth, sfbmp.PixelHeight);
                    SoftwareBitmapLuminanceSource x = new SoftwareBitmapLuminanceSource(sfbmp);
                    HybridBinarizer binarizer = new HybridBinarizer(rgb);
                    BinaryBitmap bb = new BinaryBitmap(binarizer.createBinarizer(x));

                    Dictionary<DecodeHintType, object> d = new Dictionary<DecodeHintType, object>();

                    Result r = reader.decode(bb);

                    if (r != null)
                    {
                        System.Diagnostics.Debug.WriteLine(r);
                    }


                    //QRCodeReader reader = new QRCodeReader();
                    //MediaCapture capture = new MediaCapture();
                    //await capture.InitializeAsync();
                    //Result result = null;

                    //VideoEncodingProperties resx = new VideoEncodingProperties();
                    //ImageEncodingProperties iep = new ImageEncodingProperties();
                    //iep.Height = resx.Height;
                    //iep.Width = resx.Width;

                    //WriteableBitmap wb = new WriteableBitmap((int)resx.Width, (int)resx.Height);

                    //while (result == null)
                    //{
                    //    using (InMemoryRandomAccessStream str = new InMemoryRandomAccessStream())
                    //    {
                    //        await capture.CapturePhotoToStreamAsync(iep, str);
                    //        str.Seek(0);

                    //        await wb.SetSourceAsync(str);
                    //        result = reader.decode(wb);
                    //    }
                    //}
                }
            }
        }
    }
}
