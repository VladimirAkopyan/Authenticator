using Authenticator_for_Windows.Utilities;
using Windows.UI.Xaml.Controls;

namespace Authenticator_for_Windows.Views.UserControls
{
    public sealed partial class ReleaseNotesDialog : ContentDialog
    {
        public ReleaseNotesDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ReleaseNotesManager.ShownReleaseNotes();
        }
    }
}
