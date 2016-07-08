using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Authenticator_for_Windows.Views.Pages
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
            SpinSynchronize.Begin();
        }

        private void ViewCodes_Click(object sender, RoutedEventArgs e)
        {
            mainPage.NavigateToAccountsAndClearBackStack();
        }
    }
}
