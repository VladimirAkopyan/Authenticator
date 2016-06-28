using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Domain;
using Authenticator_for_Windows.Events;
using Windows.ApplicationModel.DataTransfer;
using Settings;
using Domain.Utilities;
using Domain.Storage;

namespace Authenticator_for_Windows.Views.UserControls
{
    public sealed partial class AccountBlock : UserControl
    {
        private Account account;
        private OTP otp;
        private bool _inEditMode;
        private DispatcherTimer timer;
        
        private bool skipEditModeAnimation;

        public bool InEditMode
        {
            get
            {
                return _inEditMode;
            }
            set
            {
                _inEditMode = value;

                Storyboard storyBoard = null;

                if (value)
                {
                    storyBoard = Open;
                }
                else
                {
                    storyBoard = Close;
                }

                storyBoard.Begin();

                if (skipEditModeAnimation)
                {
                    storyBoard.SkipToFill();
                }
            }
        }

        public AccountBlock()
        {

        }

        public AccountBlock(Account account, bool flash)
        {
            Initialize(account, flash);
        }

        public AccountBlock(Account account)
        {
            Initialize(account, false);
        }

        public void Update()
        {
            if (account != null)
            {
                FadeOut.Begin();
            }
        }

        private void Initialize(Account account, bool flash)
        {
            InitializeComponent();

            this.account = account;
            otp = new OTP(account.Secret);

            DataContext = account;

            DisplayCodeFormatted();

            if (flash)
            {
                Flash.Begin();
            }
        }

        private void SlideUp_Completed(object sender, object e)
        {
            NotifyRemoved();
        }

        private void DisplayCodeFormatted()
        {
            string firstPart = otp.Code.Substring(0, 3);
            string secondPart = otp.Code.Substring(3, 3);

            CurrentCode.Text = string.Format("{0} {1}", firstPart, secondPart);
        }

        public void Remove()
        {
            SlideUp.Begin();
        }

        public void Show(bool inEditMode)
        {
            skipEditModeAnimation = !inEditMode;

            SlideDown.Begin();
        }

        private void FadeOut_Completed(object sender, object e)
        {
            DisplayCodeFormatted();

            FadeIn.Begin();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            NotifyDeleteRequested();
        }

        public event EventHandler<DeleteRequestEventArgs> DeleteRequested;
        public event EventHandler<EventArgs> Removed;
        public event EventHandler<EventArgs> CopyRequested;

        private void NotifyDeleteRequested()
        {
            if (DeleteRequested != null)
            {
                DeleteRequested(this, new DeleteRequestEventArgs(account));
            }
        }

        private void NotifyRemoved()
        {
            if (Removed != null)
            {
                Removed(this, new EventArgs());
            }
        }

        private void NotifyCopyRequested()
        {
            if (CopyRequested != null)
            {
                CopyRequested(this, new EventArgs());
            }
        }

        private void Grid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!InEditMode)
            {
                if (Flash.GetCurrentState() != ClockState.Stopped)
                {
                    Flash.Stop();
                }

                Flash.Begin();

                CopyCode();
                NotifyCopyRequested();
            }
        }

        private void CopyCode()
        {
            int clipboardType = SettingsManager.Get<int>(Setting.ClipBoardRememberType);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(otp.Code);
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

        private void Open_Completed(object sender, object e)
        {
            ShakePencil.Begin();

            if (skipEditModeAnimation)
            {
                ShakePencil.SkipToFill();

                skipEditModeAnimation = false;
            }
        }

        private async void EditPanel_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (InEditMode)
            {
                ModifyServiceDialog dialog = new ModifyServiceDialog(account);
                await dialog.ShowAsync();

                if (dialog.IsModified)
                {
                    await AccountStorage.Instance.SaveAsync(account);
                }
            }
        }
    }
}
