using Settings;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Authenticator.Views.Settings
{
    public sealed partial class General: UserControl
    {
        public General()
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
            if (sender is ToggleSwitch toggleSwitch)
            {
                SettingsManager.Save(Setting.UseNTP, toggleSwitch.IsOn);
                // If useNTP is on, the following timeout duration would apply
                SettingsManager.Save(Setting.NTPTimeout, new TimeSpan(0, 0, 2));
            }
        }

        private void AvailabilityInClipboardChecked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
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
