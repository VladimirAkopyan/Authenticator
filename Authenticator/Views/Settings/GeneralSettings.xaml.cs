using Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Authenticator.Views.Settings
{
    public sealed partial class GeneralSettings : UserControl
    {
        public GeneralSettings()
        {
            this.InitializeComponent();
            enableTimeSyncToggle.IsOn = SettingsManager.Get<bool>(Setting.UseNTP);

            int availabilityInClipboard = SettingsManager.Get<int>(Setting.ClipBoardRememberType);
            switch (availabilityInClipboard)
            {
                case 1:
                    NeverRadioButton.IsChecked = true;
                    break;
                default:
                    SettingsManager.Save(Setting.ClipBoardRememberType, 0);
                    OnExpiryRadioButton.IsChecked = true;
                    break;
            }
        }

        private void EnableTimeSyncToggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                SettingsManager.Save(Setting.UseNTP, toggleSwitch.IsOn);
            }
        }

        private void AvailabilityInClipboardChecked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (radioButton != null)
            {
                switch (radioButton.Tag.ToString())
                {
                    case "OnExpiry":
                        SettingsManager.Save(Setting.ClipBoardRememberType, 0);
                        break;
                    case "Never":
                        SettingsManager.Save(Setting.ClipBoardRememberType, 1);
                        break;
                }
            }
        }
    }
}
