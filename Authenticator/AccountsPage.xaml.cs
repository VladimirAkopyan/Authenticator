using Authenticator_for_Windows.Storage;
using Authenticator_for_Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountsPage : Page
    {
        private EntryStorage entryStorage;
        private Dictionary<Entry, EntryBlock> mappings;

        public AccountsPage()
        {
            InitializeComponent();

            entryStorage = new EntryStorage();
            mappings = new Dictionary<Entry, EntryBlock>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (Entry entry in entryStorage.Entries)
            {
                EntryBlock code = new EntryBlock(entry);
                code.DeleteRequested += Code_DeleteRequested;

                Codes.Children.Add(code);
                mappings.Add(entry, code);
            }

            StrechProgress.Begin();
            StrechProgress.Seek(new TimeSpan(0, 0, 30 - TOTPUtilities.RemainingSeconds));
        }

        private async void Code_DeleteRequested(object sender, DeleteRequestEventArgs e)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Account verwijderen",
                Content = "Weet u zeker dat u dit accoubt wilt verwijderen?\nLet op: Het verwijderen van dit account deactiveert tweestapsauthenticatie op uw account niet!",
                PrimaryButtonText = "Verwijderen",
                SecondaryButtonText = "Annuleren"
            };

            dialog.PrimaryButtonClick += delegate
            {
                entryStorage.Remove(e.Entry);
                Codes.Children.Remove(mappings.FirstOrDefault(m => m.Key == e.Entry).Value);
            };

            await dialog.ShowAsync();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            foreach (EntryBlock code in Codes.Children.Where(c => c.GetType() == typeof(EntryBlock)))
            {
                code.InEditMode = !code.InEditMode;
            }
        }
    }
}
