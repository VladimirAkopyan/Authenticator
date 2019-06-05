using Domain;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Authenticator.Views.UserControls
{
    public sealed partial class ModifyServiceDialog : ContentDialog
    {
        public bool IsModified { get; private set; }

        public ModifyServiceDialog(Account account)
        {
            InitializeComponent();
            
            DataContext = account;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(Service.Text))
            {
                Account account = (Account)DataContext;
                account.Service = Service.Text;

                IsModified = true;
            }
            else
            {
                args.Cancel = true;
            }
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Service.Select(Service.Text.Length, 0);
        }
    }
}
