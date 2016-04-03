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
    public sealed partial class Code : UserControl
    {
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

        public Code(OTP otp)
        {
            InitializeComponent();

            this.otp = otp;

            otp.PropertyChanged += Otp_PropertyChanged;

            DisplayCodeFormatted();
            Name.Text = "test@test.nl";
        }

        private void DisplayCodeFormatted()
        {
            string firstPart = otp.TOTP.Substring(0, 3);
            string secondPart = otp.TOTP.Substring(3, 3);

            CurrentCode.Text = string.Format("{0} {1}", firstPart, secondPart);
        }

        private void Otp_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DisplayCodeFormatted();
        }
    }
}
