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

namespace Authenticator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CodesPage : Page
    {
        private List<OTP> codes;

        public CodesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            OTP otp = null;

            for (int i = 0; i < 10; i++)
            {
                otp = new OTP("JBSWY3DPEHPK3PXP");
                Code code = new Code(otp);

                Codes.Children.Add(code);
            }

            StrechProgress.Begin();
            StrechProgress.Seek(new TimeSpan(0, 0, 30 - otp.RemainingSeconds));
        }
    }
}
