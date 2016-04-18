using Windows.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Authenticator_for_Windows.Views.UserControls;
using Porrey.Uwp.Ntp;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static MainPage instance;
        private bool backButtonTapped;

        public MainPage()
        {
            InitializeComponent();

            instance = this;

            // Navigate to the first page
            Navigate(typeof(AccountsPage), this);

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Contentframe.CanGoBack)
            {
                e.Handled = true;

                Contentframe.GoBack();

                SetTitle();

                backButtonTapped = true;

                if (Contentframe.Content.GetType() == typeof(AccountsPage))
                {
                    AccountsMenuItem.IsChecked = true;
                }
                else if (Contentframe.Content.GetType() == typeof(AddPage))
                {
                    AddMenuItem.IsChecked = true;
                }
                else if (Contentframe.Content.GetType() == typeof(SettingsPage))
                {
                    SettingsMenuItem.IsChecked = true;
                }
                else if (Contentframe.Content.GetType() == typeof(PrivacyDeclaration))
                {
                    SettingsMenuItem.IsChecked = true;
                }

                backButtonTapped = false;

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Contentframe.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }
        }

        internal static void ClearBanners()
        {
            instance.Bannerbar.Children.Clear();
        }

        private void OnMenuButtonClicked(object sender, RoutedEventArgs e)
        {
            Navbar.IsPaneOpen = !Navbar.IsPaneOpen;
        }

        public static void AddBanner(Banner banner)
        {
            instance.Bannerbar.Children.Add(banner);
        }

        public void Navigate(Type navigatepage, object parameter = null, bool addToBackStack = true)
        {
            Navbar.IsPaneOpen = false;

            if (Contentframe != null)
            {
                Contentframe.Navigate(navigatepage, parameter);

                if (!addToBackStack)
                {
                    Contentframe.BackStack.RemoveAt(Contentframe.BackStackDepth - 1);
                }

                SetTitle();

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Contentframe.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }
        }

        public void SetTitle()
        {
            if (Contentframe.Content is Page)
            {
                Page page = (Page)Contentframe.Content;

                if (page.Tag != null && page.Tag.GetType() == typeof(string))
                {
                    TextHeader.Text = page.Tag.ToString();
                }
            }
        }

        private void AccountsMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!backButtonTapped)
            {
                Navigate(typeof(AccountsPage), this);
            }
        }

        private void AddMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!backButtonTapped)
            {
                Navigate(typeof(AddPage), new object[] { this });
            }
        }

        private void SettingsMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!backButtonTapped)
            {
                Navigate(typeof(SettingsPage), this);
            }
        }

        public void BeginAnimateAddAccount()
        {
            AddAccountFlash.Begin();
        }

        public void EndAnimateAddAccount()
        {
            AddAccountFlash.Stop();
        }
    }
}
