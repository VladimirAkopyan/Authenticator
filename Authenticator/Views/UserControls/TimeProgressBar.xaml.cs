using Domain.Utilities;
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

namespace Authenticator_for_Windows.Views.UserControls
{
    public sealed partial class TimeProgressBar : UserControl
    {
        public TimeProgressBar()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> TimeElapsed;

        private void NotifyTimeElapsed()
        {
            if (TimeElapsed != null)
            {
                TimeElapsed(this, new EventArgs());
            }
        }

        private void StrechProgress_Completed(object sender, object e)
        {
            NotifyTimeElapsed();

            StrechProgress.Stop();
            StrechProgress.Begin();
            StrechProgress.Seek(new TimeSpan((30 * TimeSpan.TicksPerSecond) - TOTPUtilities.RemainingTicks));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StrechProgress.Begin();
            StrechProgress.Seek(new TimeSpan((30 * TimeSpan.TicksPerSecond) - TOTPUtilities.RemainingTicks));
        }
    }
}
