using Authenticator_for_Windows.Events;
using Domain.Storage;
using Authenticator_for_Windows.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Domain.Utilities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Domain;

namespace Authenticator_for_Windows.Views.Pages
{
    public sealed partial class AccountsPage : Page
    {
        private Dictionary<Account, AccountBlock> mappings;
        private Account selectedAccount;
        private IReadOnlyList<Account> accounts;
        private MainPage mainPage;
        private ObservableCollection<AccountBlock> accountBlocks;
        private AccountBlock removedAccountBlock;
        private DispatcherTimer undoTimer;
        private bool inEditMode;
        private int removedIndex;
        private int reorderFrom;
        private int reorderTo;
        private const int UNDO_MESSAGE_VISIBLE_SECONDS = 5;

        public AccountsPage()
        {
            InitializeComponent();

            mappings = new Dictionary<Account, AccountBlock>();
            undoTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, UNDO_MESSAGE_VISIBLE_SECONDS)
            };
            undoTimer.Tick += UndoTimer_Tick;
        }

        private void UndoTimer_Tick(object sender, object e)
        {
            CloseUndo.Begin();

            undoTimer.Stop();
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

        private async void HandleReorder()
        {
            await AccountStorage.Instance.ReorderAsync(reorderFrom, reorderTo);
        }

        private void Code_Removed(object sender, EventArgs e)
        {
            OpenUndo.Begin();

            AccountBlock accountBlock = sender as AccountBlock;
            removedAccountBlock = accountBlock;
            removedIndex = accountBlocks.IndexOf(accountBlock);

            if (accountBlock != null)
            {
                accountBlocks.Remove(accountBlock);
            }

            CheckEntries();

            undoTimer.Start();
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

                    ReorderClose.Begin();
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

        private void TimeProgressBar_TimeElapsed(object sender, EventArgs e)
        {
            foreach (AccountBlock accountBlock in Codes.Items.Where(c => c.GetType() == typeof(AccountBlock)))
            {
                accountBlock.Update();
            }
        }

        private void Edit_Checked(object sender, RoutedEventArgs e)
        {
            ChangeEditState(true);
        }

        private void Edit_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeEditState(false);
        }

        private void ChangeEditState(bool inEdit)
        {
            inEditMode = inEdit;

            Codes.CanReorderItems = inEdit;

            foreach (AccountBlock accountBlock in Codes.Items.Where(c => c.GetType() == typeof(AccountBlock)))
            {
                accountBlock.InEditMode = inEdit;
            }

            if (inEdit)
            {
                ReorderOpen.Begin();
            }
            else
            {
                ReorderClose.Begin();
            }
        }

        private async void ButtonUndo_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await AccountStorage.Instance.UndoRemoveAsync();

            removedAccountBlock.InEditMode = inEditMode;
            accountBlocks.Insert(removedIndex, removedAccountBlock);
            removedAccountBlock.Show();

            CheckEntries();
            CloseUndo.Begin();
        }
    }
}
