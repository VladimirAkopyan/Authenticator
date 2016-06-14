using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ZXing;
using ZXing.Mobile;

namespace Authenticator_for_Windows.Views.Pages
{
    public sealed partial class ScanPage : Page
    {
        private bool isNewInstance = false;

        public ScanPage()
        {
            isNewInstance = true;
            InitializeComponent();
        }

        public static MobileBarcodeScanningOptions ScanningOptions { get; set; }
        public static MobileBarcodeScannerBase Scanner { get; set; }
        public static UIElement CustomOverlay { get; set; }
        public static string TopText { get; set; }
        public static string BottomText { get; set; }
        public static bool UseCustomOverlay { get; set; }
        public static bool ContinuousScanning { get; set; }
        public static bool AccessDenied { get; private set; }

        public static Result LastScanResult { get; set; }

        public static Action<Result> ResultFoundAction { get; set; }

        public static event Action<bool> OnRequestTorch;
        public static event Action OnRequestToggleTorch;
        public static event Action OnRequestAutoFocus;
        public static event Action OnRequestCancel;
        public static event Func<bool> OnRequestIsTorchOn;
        public static event Action OnRequestPauseAnalysis;
        public static event Action OnRequestResumeAnalysis;

        public static bool RequestIsTorchOn()
        {
            var evt = OnRequestIsTorchOn;
            return evt != null && evt();
        }

        public static void RequestTorch(bool on)
        {
            var evt = OnRequestTorch;
            if (evt != null)
                evt(on);
        }

        public static void RequestToggleTorch()
        {
            var evt = OnRequestToggleTorch;
            if (evt != null)
                evt();
        }

        public static void RequestAutoFocus()
        {
            var evt = OnRequestAutoFocus;
            if (evt != null)
                evt();
        }

        public static void RequestCancel()
        {
            var evt = OnRequestCancel;
            if (evt != null)
                evt();
        }

        public static void RequestPauseAnalysis()
        {
            var evt = OnRequestPauseAnalysis;
            if (evt != null)
                evt();
        }

        public static void RequestResumeAnalysis()
        {
            var evt = OnRequestResumeAnalysis;
            if (evt != null)
                evt();
        }

        void RequestAutoFocusHandler()
        {
            if (scannerControl != null)
                scannerControl.AutoFocus();
        }

        void RequestTorchHandler(bool on)
        {
            if (scannerControl != null)
                scannerControl.Torch(on);
        }

        void RequestToggleTorchHandler()
        {
            if (scannerControl != null)
                scannerControl.ToggleTorch();
        }

        async Task RequestCancelHandler()
        {
            if (scannerControl != null)
                await scannerControl.Cancel();
        }

        bool RequestIsTorchOnHandler()
        {
            if (scannerControl != null)
                return scannerControl.IsTorchOn;

            return false;
        }

        void RequestPauseAnalysisHandler()
        {
            if (scannerControl != null)
                scannerControl.PauseAnalysis();
        }

        void RequestResumeAnalysisHandler()
        {
            if (scannerControl != null)
                scannerControl.ResumeAnalysis();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            scannerControl.TopText = TopText;
            scannerControl.BottomText = BottomText;
            
            scannerControl.CustomOverlay = CustomOverlay;
            scannerControl.UseCustomOverlay = UseCustomOverlay;

            scannerControl.ScanningOptions = ScanningOptions;
            scannerControl.ContinuousScanning = ContinuousScanning;

            OnRequestAutoFocus += RequestAutoFocusHandler;
            OnRequestTorch += RequestTorchHandler;
            OnRequestToggleTorch += RequestToggleTorchHandler;
            OnRequestCancel += ScanPage_OnRequestCancel;
            OnRequestIsTorchOn += RequestIsTorchOnHandler;
            OnRequestPauseAnalysis += RequestPauseAnalysisHandler;
            OnRequestResumeAnalysis += RequestResumeAnalysisHandler;

            try
            {
                AccessDenied = false;

                await scannerControl.StartScanningAsync(HandleResult, ScanningOptions);
                
                if (!isNewInstance && Frame.CanGoBack)
                    Frame.GoBack();

                isNewInstance = false;
            }
            catch (UnauthorizedAccessException)
            {
                AccessDenied = true;

                Frame.GoBack();
            }

            base.OnNavigatedTo(e);
        }

        private async void ScanPage_OnRequestCancel()
        {
            await RequestCancelHandler();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (scannerControl != null)
            {
                OnRequestAutoFocus -= RequestAutoFocusHandler;
                OnRequestTorch -= RequestTorchHandler;
                OnRequestToggleTorch -= RequestToggleTorchHandler;
                OnRequestCancel -= ScanPage_OnRequestCancel;
                OnRequestIsTorchOn -= RequestIsTorchOnHandler;
                OnRequestPauseAnalysis -= RequestPauseAnalysisHandler;
                OnRequestResumeAnalysis -= RequestResumeAnalysisHandler;

                await scannerControl.StopScanningAsync();
            }

            base.OnNavigatingFrom(e);
        }

        private async void HandleResult(Result result)
        {
            LastScanResult = result;

            var evt = ResultFoundAction;
            if (evt != null)
                evt(LastScanResult);

            if (!ContinuousScanning)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    if (Frame.CanGoBack)
                        Frame.GoBack();
                });
            }
        }
    }
}
