using System.IO;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PrivacyDeclaration : Page
    {
        public PrivacyDeclaration()
        {
            InitializeComponent();

            string html = File.ReadAllText("Resources/PrivacyDeclaration.html");

            WebView.NavigateToString(html);
        }
    }
}
