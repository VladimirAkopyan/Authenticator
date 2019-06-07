using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Authenticator.Views.Settings
{
    public sealed partial class GeneralSettings : UserControl
    {
        public GeneralSettings()
        {
            this.InitializeComponent();
        }

        private void NTPTimeoutSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }

        private void EnableTimeSyncToggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn)
                {

                }
                else
                {

                }
            }
        }

        private void ClipboardAvailabilityChecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
