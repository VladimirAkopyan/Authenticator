using Authenticator_for_Windows.Events;
using Domain.Storage;
using Domain.Extensions;
using Authenticator_for_Windows.Views.UserControls;
using Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Domain.Utilities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI.Xaml.Media.Animation;
using Domain;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Authenticator_for_Windows.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountsPage : Page
    {
        private Dictionary<Account, AccountBlock> mappings;
        private Account selectedAccount;
        private IReadOnlyList<Account> accounts;
        private MainPage mainPage;
        private ObservableCollection<AccountBlock> accountBlocks;
        private int reorderFrom;
        private int reorderTo;

        public AccountsPage()
        {
            InitializeComponent();

            mappings = new Dictionary<Account, AccountBlock>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mainPage = (MainPage)e.Parameter;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            accounts = await AccountStorage.Instance.GetAllAsync();

            long currentTicks = TOTPUtilities.RemainingTicks;
            TimeSpan remainingTime = new TimeSpan(TOTPUtilities.RemainingTicks);

            accountBlocks = new ObservableCollection<AccountBlock>();

            PageGrid.Children.Remove(LoaderProgressBar);

            foreach (Account account in accounts)
            {
                AccountBlock code = new AccountBlock(account);
                code.DeleteRequested += Code_DeleteRequested;
                code.CopyRequested += Code_CopyRequested;
                code.Removed += Code_Removed;

                accountBlocks.Add(code);
                mappings.Add(account, code);
            }

            accountBlocks.CollectionChanged += AccountBlocks_CollectionChanged;
            Codes.ItemsSource = accountBlocks;

            CheckEntries();
        }

        private void AccountBlocks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    reorderFrom = e.OldStartingIndex;
                    break;
                case NotifyCollectionChangedAction.Add:
                    reorderTo = e.NewStartingIndex;
                    break;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                HandleReorder();
            }
        }

        private void HandleReorder()
        {
            AccountStorage.Instance.Reorder(reorderFrom, reorderTo);
        }

        private void Code_Removed(object sender, EventArgs e)
        {
            accountBlocks.Remove((AccountBlock)sender);
            CheckEntries();
        }

        private void CheckEntries()
        {
            if (accounts != null)
            {
                if (accounts.Count == 0)
                {
                    NoAccountsGrid.Visibility = Visibility.Visible;
                    CommandBar.Visibility = Visibility.Collapsed;

                    mainPage.BeginAnimateAddAccount();
                }
                else
                {
                    NoAccountsGrid.Visibility = Visibility.Collapsed;
                    CommandBar.Visibility = Visibility.Visible;

                    mainPage.EndAnimateAddAccount();
                }
            }
        }

        private void Code_CopyRequested(object sender, EventArgs e)
        {
            CodeCopiedNotification.Animate();
        }

        private async void Code_DeleteRequested(object sender, DeleteRequestEventArgs e)
        {
            selectedAccount = e.Account;

            await ConfirmDialog.ShowAsync();
        }

        private async void ConfirmDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            KeyValuePair<Account, AccountBlock> account = mappings.FirstOrDefault(m => m.Key == selectedAccount);

            await AccountStorage.Instance.RemoveAsync(account.Key);

            account.Value.Remove();
        }

        private void Edit_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Codes.CanReorderItems = Edit.IsChecked.HasValue ? Edit.IsChecked.Value : false;

            foreach (AccountBlock accountBlock in Codes.Items.Where(c => c.GetType() == typeof(AccountBlock)))
            {
                accountBlock.InEditMode = !accountBlock.InEditMode;
            }

            if (Edit.IsChecked.HasValue)
            {
                if (Edit.IsChecked.Value)
                {
                    ReorderOpen.Begin();
                }
                else
                {
                    ReorderClose.Begin();
                }
            }
        }

        private void TimeProgressBar_TimeElapsed(object sender, EventArgs e)
        {
            foreach (AccountBlock accountBlock in Codes.Items.Where(c => c.GetType() == typeof(AccountBlock)))
            {
                accountBlock.Update();
            }
        }
    }
}
