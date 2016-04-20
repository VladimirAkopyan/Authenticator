using Domain.Events;
using Domain.Storage;
using Domain.Extensions;
using Settings;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Domain.Views.UserControls
{
    public sealed partial class AccountBlock : UserControl
    {
        private Account account;
        private OTP otp;
        private bool _inEditMode;

        public bool InEditMode
        {
            get
            {
                return _inEditMode;
            }
            set
            {
                _inEditMode = value;

                if (value)
                {
                    Open.Begin();
                }
                else
                {
                    Close.Begin();
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
            FadeOut.Begin();
        }

        private void Initialize(Account account, bool flash)
        {
            InitializeComponent();

            this.account = account;
            otp = new OTP(account.Secret);

            DisplayCodeFormatted();
            EntryUsername.Text = account.Username;
            EntryService.Text = account.Service;
            FadeOut.Completed += FadeOut_Completed;
            SlideUp.Completed += SlideUp_Completed;

            if (flash)
            {
                Flash.Begin();
            }
        }


        private void SlideUp_Completed(object sender, object e)
        {
            StackPanel stackPanel = (StackPanel)Parent;

            stackPanel.Children.Remove(stackPanel);

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
        public event EventHandler<CopyRequestEventArgs> CopyRequested;

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
                CopyRequested(this, new CopyRequestEventArgs(otp.Code));
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

                NotifyCopyRequested();
            }
        }
    }
}
