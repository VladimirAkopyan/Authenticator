using Authenticator.Storage;
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
        private EntryStorage entryStorage;
        private List<Code> codes;

        public CodesPage()
        {
            InitializeComponent();

            entryStorage = new EntryStorage();

            codes = new List<Code>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            OTP otp = null;

            for (int i = 0; i < 10; i++)
            {
                otp = new OTP("JBSWY3DPEHPK3PXP");
                Code code = new Code(otp);

                Codes.Children.Add(code);
                codes.Add(code);
            }

            StrechProgress.Begin();
            StrechProgress.Seek(new TimeSpan(0, 0, 30 - otp.RemainingSeconds));
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            foreach (Code code in codes)
            {
                code.InEditMode = !code.InEditMode;
            }
        }
    }
}
