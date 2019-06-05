using Windows.UI.Xaml.Controls;

namespace Authenticator.Views.UserControls
{
    public sealed partial class ShowUserKeyDialog : ContentDialog
    {
        public ShowUserKeyDialog(string userKey)
        {
            InitializeComponent();

            UserKey.Text = userKey;
        }
    }
}
