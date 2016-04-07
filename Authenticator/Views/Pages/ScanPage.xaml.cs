using Authenticator_for_Windows.Storage;
using Authenticator_for_Windows.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ZXing.Mobile;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScanPage : Page
    {
        private static bool waitingForReturn;

        public static bool IsAllowedToScan { get; set; }

        public static Entry Entry { get; set; }

        public ScanPage()
        {
            InitializeComponent();
        }

        private void Scan()
        {
            MobileBarcodeScanner scanner = new MobileBarcodeScanner(Dispatcher)
            {
                UseCustomOverlay = false,
                TopText = "Positioneer de QR-code tussen de strepen",
                BottomText = "De QR-code wordt auomatisch gescand.\n\r\n\rTik of klik op de terugknop om te annuleren.",
            };

            scanner.Scan().ContinueWith(t =>
            {
                if (t.Result != null)
                {
                    HandleScanResult(t.Result);
                }
            });
        }

        private void HandleScanResult(ZXing.Result result)
        {
            Entry = TOTPUtilities.UriToEntry(result.Text);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!waitingForReturn && IsAllowedToScan)
            {
                IsAllowedToScan = false;
                waitingForReturn = true;

                Scan();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            waitingForReturn = e.SourcePageType == typeof(ZXing.Mobile.ScanPage);
        }
    }
}
