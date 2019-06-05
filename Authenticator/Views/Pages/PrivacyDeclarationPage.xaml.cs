using System.Globalization;
using System.IO;
using Windows.UI.Xaml.Controls;

namespace Authenticator.Views.Pages
{
    public sealed partial class PrivacyDeclaration : Page
    {
        private const string FILENAME_STRUCTURE = "Resources/PrivacyDeclaration.{0}.html";
        private const string DEFAULT_LANGUAGE = "en";

        public PrivacyDeclaration()
        {
            InitializeComponent();

            string fileName = string.Format(FILENAME_STRUCTURE, DEFAULT_LANGUAGE);

            if (File.Exists(string.Format(FILENAME_STRUCTURE, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)))
            {
                fileName = string.Format(FILENAME_STRUCTURE, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
            }

            string html = File.ReadAllText(fileName);

            WebView.NavigateToString(html);
        }
    }
}
