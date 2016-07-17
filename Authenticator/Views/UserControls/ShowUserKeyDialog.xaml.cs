using Windows.UI.Xaml.Controls;

namespace Authenticator_for_Windows.Views.UserControls
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
