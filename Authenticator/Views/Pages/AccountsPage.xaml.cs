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
using Synchronization;
using System.Threading.Tasks;
using Synchronization.Exceptions;
using Settings;
using Windows.ApplicationModel.Resources;

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
        private bool didUndo;
        private int removedIndex;
        private int reorderFrom;
        private int reorderTo;
        private const int UNDO_MESSAGE_VISIBLE_SECONDS = 5;

        public AccountsPage()
        {
            InitializeComponent();

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

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            AccountStorage.Instance.SynchronizationStarted -= SynchronizationStarted;
            AccountStorage.Instance.SynchronizationCompleted -= SynchronizationCompleted;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsManager.Get<bool>(Setting.UseCloudSynchronization) && AccountStorage.Instance.HasSynchronizer)
            {
                AccountStorage.Instance.SynchronizationStarted += SynchronizationStarted;
                AccountStorage.Instance.SynchronizationCompleted += SynchronizationCompleted;

                Synchronize.Visibility = Visibility.Visible;
            }

            await LoadAccounts();

            PageGrid.Children.Remove(LoaderProgressBar);

            if (AccountStorage.Instance.IsSynchronizing)
            {
                Synchronize.StartAnimationAndDisable();
                Edit.IsEnabled = false;
            }
        }

        private async Task LoadAccounts()
        {
            accounts = await AccountStorage.Instance.GetAllAsync();

            long currentTicks = TOTPUtilities.RemainingTicks;
            TimeSpan remainingTime = new TimeSpan(TOTPUtilities.RemainingTicks);

            accountBlocks = new ObservableCollection<AccountBlock>();
            mappings = new Dictionary<Account, AccountBlock>();

            foreach (Account account in accounts)
            {
                AccountBlock code = new AccountBlock(account, mainPage);
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

        private async void SynchronizationCompleted(object sender, SynchronizationResult e)
        {
            if (e.HasChanges)
            {
                await LoadAccounts();
            }
            else if (!e.Successful)
            {
                RevertAndReload();
            }

            Edit.IsEnabled = true;
            Codes.IsEnabled = true;
            
            Synchronize.StopAnimationAndEnable();
        }

        private void SynchronizationStarted(object sender, EventArgs e)
        {
            Synchronize.StartAnimationAndDisable();

            Edit.IsEnabled = false;
            Codes.IsEnabled = false;
        }

        private void AccountBlocks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Check if the collection was changed because of an undo. If that's the case, there's no need to do anything.
            if (!didUndo)
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
        }

        private async void HandleReorder()
        {
            try
            {
                await AccountStorage.Instance.ReorderAsync(reorderFrom, reorderTo);
            }
            catch (StaleException)
            {
                RevertAndReload();

                MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("ChangesDetectedRedoMove"), true));
            }
            catch (NetworkException)
            {
                RevertAndReload();

                MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("NoInternetChangesRolledBack"), true));
            }
            catch (Exception e)
            {
                mainPage.Navigate(typeof(ErrorPage), e);
            }
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
                    Codes.Visibility = Visibility.Collapsed;
                    NoAccountsGrid.Visibility = Visibility.Visible;
                    CommandBar.Visibility = Visibility.Collapsed;
                    Edit.Visibility = Visibility.Collapsed;

                    mainPage.BeginAnimateAddAccount();

                    ReorderClose.Begin();
                }
                else
                {
                    Codes.Visibility = Visibility.Visible;
                    NoAccountsGrid.Visibility = Visibility.Collapsed;
                    CommandBar.Visibility = Visibility.Visible;
                    Edit.Visibility = Visibility.Visible;

                    mainPage.EndAnimateAddAccount();
                }
            }

            if (SettingsManager.Get<bool>(Setting.UseCloudSynchronization))
            {
                CommandBar.Visibility = Visibility.Visible;
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

        private async void RevertAndReload()
        {
            await LoadAccounts();
            
            Codes.CanReorderItems = false;
            Edit.IsChecked = false;

            Synchronize.StopAnimationAndEnable();
        }

        private async void ConfirmDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                KeyValuePair<Account, AccountBlock> account = mappings.FirstOrDefault(m => m.Key == selectedAccount);

                await AccountStorage.Instance.RemoveAsync(account.Key);

                account.Value.Remove();
            }
            catch (StaleException)
            {
                RevertAndReload();

                MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("ChangesDetectedRedoDelete"), true));
            }
            catch (NetworkException)
            {
                RevertAndReload();
                
                MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("NoInternetChangesRolledBack"), true));
            }
            catch (Exception ex)
            {
                mainPage.Navigate(typeof(ErrorPage), ex);
            }
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
            didUndo = true;

            try
            {
                await AccountStorage.Instance.UndoRemoveAsync();

                removedAccountBlock.Show(inEditMode);
                accountBlocks.Insert(removedIndex, removedAccountBlock);

                CheckEntries();
                CloseUndo.Begin();
            }
            catch (StaleException)
            {
                RevertAndReload();

                MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("ChangesDetectedRedoUndo"), true));
            }
            catch (NetworkException)
            {
                RevertAndReload();

                MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("NoInternetChangesRolledBack"), true));
            }
            catch (Exception ex)
            {
                mainPage.Navigate(typeof(ErrorPage), ex);
            }

            didUndo = false;
        }

        private async void Synchronize_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Edit.IsChecked = false;
            Edit.IsEnabled = false;

            try
            {
                await AccountStorage.Instance.UpdateLocalFromRemote();
            }
            catch (NetworkException)
            {
                RevertAndReload();

                MainPage.AddBanner(new Banner(BannerType.Danger, ResourceLoader.GetForCurrentView().GetString("NoInternetConnection"), true));
            }
            catch (Exception ex)
            {
                mainPage.Navigate(typeof(ErrorPage), ex);
            }
        }
    }
}
