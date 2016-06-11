using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Authenticator_for_Windows.Views.Pages
{
    public sealed partial class AccessDeniedDialog : ContentDialog
    {
        public AccessDeniedDialog()
        {
            this.InitializeComponent();
        }

        private async void CameraSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool result = await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-webcam"));
        }
    }
}
