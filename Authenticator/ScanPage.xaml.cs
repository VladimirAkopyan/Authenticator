using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZXing;
using ZXing.Mobile;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScanPage : Page
    {
        private Frame contentFrame;

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
                if (t.Result != null)
                    HandleScanResult(t.Result);
            });
        }

        private void HandleScanResult(ZXing.Result result)
        {
            string msg = "";

            if (result != null && !string.IsNullOrEmpty(result.Text))
                msg = "Found Barcode: " + result.Text;
            else
                msg = "Scanning Canceled!";

            Debug.WriteLine(msg);

        }

        async Task MessageBox(string text)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                var dialog = new MessageDialog(text);
                await dialog.ShowAsync();
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            contentFrame = (Frame)e.Parameter;

            Scan();
        }
    }
}
