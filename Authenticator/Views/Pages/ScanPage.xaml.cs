using Authenticator_for_Windows.Storage;
using Authenticator_for_Windows.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
        private Frame contentFrame;
        private MainPage mainPage;
        private static bool waitingForReturn;
        private static bool didScan;
        private static Entry entry;

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
                RootFrame = contentFrame
            };

            scanner.Scan().ContinueWith(t =>
            {
                didScan = true;

                if (t.Result != null)
                {
                    HandleScanResult(t.Result);
                }
            });
        }

        private void HandleScanResult(ZXing.Result result)
        {
            entry = TOTPUtilities.UriToEntry(result.Text);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            object[] parameters = (object[])e.Parameter;
            
            contentFrame = (Frame)parameters[0];

            if (!waitingForReturn && !didScan)
            {
                waitingForReturn = true;

                Scan();
            }
            else
            {
                // We're redirected from the scan page
                contentFrame = (Frame)parameters[0];
                mainPage = (MainPage)parameters[1];

                mainPage.Navigate(typeof(AddPage), new object[] { contentFrame, mainPage, entry });
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            waitingForReturn = e.SourcePageType == typeof(ZXing.Mobile.ScanPage);
        }
    }
}
