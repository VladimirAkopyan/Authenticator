using Authenticator.Storage;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Authenticator
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

        public EntryBlock(Entry entry)
        {
            InitializeComponent();

            this.entry = entry;
            otp = new OTP(entry.Secret);

            otp.PropertyChanged += Otp_PropertyChanged;

            DisplayCodeFormatted();
            EntryName.Text = entry.Name;
            FadeOut.Completed += FadeOut_Completed;
        }

        private void FadeIn_Completed(object sender, object e)
        {
            throw new NotImplementedException();
        }

        private void DisplayCodeFormatted()
        {
            string firstPart = otp.Code.Substring(0, 3);
            string secondPart = otp.Code.Substring(3, 3);

            CurrentCode.Text = string.Format("{0} {1}", firstPart, secondPart);
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
