using Authenticator_for_Windows.Events;
using Authenticator_for_Windows.Storage;
using Authenticator_for_Windows.Utilities;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Authenticator_for_Windows.Views.UserControls
{
    public sealed partial class EntryBlock : UserControl
    {
        private Entry entry;
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

        public EntryBlock()
        {

        }

        public EntryBlock(Entry entry, bool flash)
        {
            Initialize(entry, flash);
        }

        public EntryBlock(Entry entry)
        {
            Initialize(entry, false);
        }

        private void Initialize(Entry entry, bool flash)
        {
            InitializeComponent();

            this.entry = entry;
            otp = new OTP(entry.Secret);

            otp.PropertyChanged += Otp_PropertyChanged;

            DisplayCodeFormatted();
            EntryName.Text = entry.Name;
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

        private void Otp_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FadeOut.Begin();
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

        private void NotifyDeleteRequested()
        {
            if (DeleteRequested != null)
            {
                DeleteRequested(this, new DeleteRequestEventArgs(entry));
            }
        }
    }
}
