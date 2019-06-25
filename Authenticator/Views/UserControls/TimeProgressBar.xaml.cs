using Domain.Utilities;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Authenticator.Views.UserControls
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
