using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Authenticator.Views.UserControls
{
    public sealed partial class SynchronizeAppBarButton : AppBarButton
    {
        public SynchronizeAppBarButton()
        {
            InitializeComponent();
        }

        public void StartAnimationAndDisable()
        {
            SpinSynchronize.Begin();
            IsEnabled = false;
        }

        public void StopAnimationAndEnable()
        {
            SpinSynchronize.Stop();
            IsEnabled = true;
        }

        private void Synchronize_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StartAnimationAndDisable();
        }
    }
}
