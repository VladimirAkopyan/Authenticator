using Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Authenticator.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetupSynchronizationFinishedPage : Page
    {
        private MainPage mainPage;

        public SetupSynchronizationFinishedPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mainPage = (MainPage)e.Parameter;

            mainPage.ClearBackStack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsManager.Save(Setting.UseCloudSynchronization, true);

            SpinSynchronize.Begin();
        }

        private void ViewCodes_Click(object sender, RoutedEventArgs e)
        {
            mainPage.NavigateToAccountsAndClearBackStack();
        }
    }
}
