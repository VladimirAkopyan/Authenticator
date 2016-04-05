﻿using Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private MainPage mainPage;

        public SettingsPage()
        {
            InitializeComponent();

            int rememberTime = SettingsManager.Get<int>(Setting.ClipBoardRememberType);

            if (rememberTime <= ClipboardTime.Items.Count)
            {
                ClipboardTime.SelectedIndex = rememberTime;
            }
        }

        private void ClipboardTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingsManager.Save(Setting.ClipBoardRememberType, ClipboardTime.SelectedIndex);
        }

        private void PrivacyDeclaration_Click(object sender, RoutedEventArgs e)
        {
            mainPage.Navigate(typeof(PrivacyDeclaration));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mainPage = (MainPage)e.Parameter;
        }
    }
}