using Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows.Views.UserControls
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
