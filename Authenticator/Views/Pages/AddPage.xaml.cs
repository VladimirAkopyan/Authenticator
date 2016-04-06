using Authenticator_for_Windows.Storage;
using Authenticator_for_Windows.Views.UserControls;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddPage : Page
    {
        private EntryStorage entryStorage;
        private MainPage mainPage;
        private Frame contentFrame;

        public AddPage()
        {
            InitializeComponent();

            entryStorage = new EntryStorage();

            CheckForCamera();
        }

        private async void CheckForCamera()
        {
            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);

            if (devices.Count > 1)
            {
                CommandBar.Visibility = Visibility.Visible;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            object[] parameters = (object[])e.Parameter;
            mainPage = (MainPage)parameters[0];
            contentFrame = (Frame)parameters[1];

            if (parameters.Length >= 3)
            {
                Entry entry = (Entry)parameters[2];

                EntryName.Text = entry.Name;
                EntryCode.Text = entry.Secret;

                Save_Click(null, null);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            MainPage.ClearBanners();
        }

        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            ScanPage.IsAllowedToScan = true;

            mainPage.Navigate(typeof(ScanPage), new object[] { contentFrame, mainPage });
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            EntryBlockPanel.Visibility = Visibility.Collapsed;

            MainPage.ClearBanners();
            EntryBlockPanel.Children.Clear();

            bool valid = true;

            if (string.IsNullOrWhiteSpace(EntryName.Text) || string.IsNullOrWhiteSpace(EntryCode.Text))
            {
                MainPage.AddBanner(new Banner(BannerType.Danger, "U heeft niet alle velden ingevuld.", true));

                valid = false;
            }

            if (valid)
            {
                if (!EntryCode.Text.All(char.IsLetterOrDigit))
                {
                    MainPage.AddBanner(new Banner(BannerType.Danger, "De code bevat ongeldige karaketers.", true));

                    valid = false;
                }

                if (valid)
                {
                    Entry entry = new Entry()
                    {
                        Name = EntryName.Text,
                        Secret = EntryCode.Text
                    };

                    await entryStorage.SaveAsync(entry);

                    EntryBlock entryBlock = new EntryBlock(entry, true);

                    MainPage.AddBanner(new Banner(BannerType.Success, "Uw code is opgeslagen. De huidige code wordt nu weergegeven.", true));
                    EntryBlockPanel.Children.Add(entryBlock);

                    EntryBlockPanel.Visibility = Visibility.Visible;

                    EntryName.Text = "";
                    EntryCode.Text = "";
                }
            }
        }
    }
}
