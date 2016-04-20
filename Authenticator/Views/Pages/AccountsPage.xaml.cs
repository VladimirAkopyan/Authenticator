using Domain.Events;
using Domain.Storage;
using Domain.Extensions;
using Domain.Views.UserControls;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Domain.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountsPage : Page
    {
        private Dictionary<Account, AccountBlock> mappings;
        private DispatcherTimer timer;
        private Account selectedAccount;
        private IReadOnlyList<Account> accounts;
        private MainPage mainPage;

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

            List<Account> invalidAccounts = new List<Account>();

            foreach (Account account in accounts)
            {
                try
                {
                    OTP otp = new OTP(account.Secret);
                }
                catch
                {
                    invalidAccounts.Add(account);
                }
            }

            if (invalidAccounts.Count > 0)
            {
                foreach (Account invalidAccount in invalidAccounts)
                {
                    await AccountStorage.Instance.RemoveAsync(invalidAccount);
                }
            }

            PageGrid.Children.Remove(LoaderProgressBar);

            foreach (Account account in accounts)
            {
                AccountBlock code = new AccountBlock(account);
                code.DeleteRequested += Code_DeleteRequested;
                code.CopyRequested += Code_CopyRequested;
                code.Removed += Code_Removed;

                Codes.Children.Add(code);
                mappings.Add(account, code);
            }

            CheckEntries();

            StrechProgress.Completed += StrechProgress_Completed;

            StrechProgress.Begin();
            StrechProgress.Seek(new TimeSpan((30 * TimeSpan.TicksPerSecond) - TOTPUtilities.RemainingTicks));
        }

        private void Code_Removed(object sender, EventArgs e)
        {
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

        private void StrechProgress_Completed(object sender, object e)
        {
            foreach (AccountBlock entryBlock in Codes.Children.Where(c => c.GetType() == typeof(AccountBlock)))
            {
                entryBlock.Update();
            }

            StrechProgress.Stop();
            StrechProgress.Begin();
            StrechProgress.Seek(new TimeSpan((30 * TimeSpan.TicksPerSecond) - TOTPUtilities.RemainingTicks));
        }

        private void Code_CopyRequested(object sender, CopyRequestEventArgs e)
        {
            int clipboardType = SettingsManager.Get<int>(Setting.ClipBoardRememberType);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(e.Code);
            Clipboard.SetContent(dataPackage);

            // Type 1 = dynamic, type 2 = forever

            if (clipboardType == 0)
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Interval = new TimeSpan(0, 0, TOTPUtilities.RemainingSeconds);
                }
                else
                {
                    timer = new DispatcherTimer()
                    {
                        Interval = new TimeSpan(0, 0, TOTPUtilities.RemainingSeconds)
                    };

                    timer.Tick += Timer_Tick;
                }

                timer.Start();
            }

            Copied.Begin();
        }

        private void Timer_Tick(object sender, object e)
        {
            try
            {
                Clipboard.Clear();
            }
            catch (Exception)
            {
                // Cannot clear the clipboard (perhaps it's in use)
            }

            timer.Stop();
        }

        private async void Code_DeleteRequested(object sender, DeleteRequestEventArgs e)
        {
            selectedAccount = e.Account;

            await ConfirmDialog.ShowAsync();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            foreach (AccountBlock entryBlock in Codes.Children.Where(c => c.GetType() == typeof(AccountBlock)))
            {
                entryBlock.InEditMode = !entryBlock.InEditMode;
            }
        }

        private async void ConfirmDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            KeyValuePair<Account, AccountBlock> account = mappings.FirstOrDefault(m => m.Key == selectedAccount);

            await AccountStorage.Instance.RemoveAsync(account.Key);

            account.Value.Remove();
        }
    }
}
