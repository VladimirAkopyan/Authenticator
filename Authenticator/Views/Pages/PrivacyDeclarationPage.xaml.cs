using System.Globalization;
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
        private const string FILENAME_STRUCTURE = "Resources/PrivacyDeclaration.{0}.html";
        private const string DEFAULT_LANGUAGE = "en-US";

        public PrivacyDeclaration()
        {
            InitializeComponent();

            string fileName = string.Format(FILENAME_STRUCTURE, DEFAULT_LANGUAGE);

            if (File.Exists(string.Format(FILENAME_STRUCTURE, CultureInfo.CurrentUICulture.Name)))
            {
                fileName = string.Format(FILENAME_STRUCTURE, CultureInfo.CurrentUICulture.Name);
            }

            string html = File.ReadAllText(fileName);

            WebView.NavigateToString(html);
        }
    }
}
