using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Authenticator_for_Windows.Views.UserControls
{
    public sealed partial class ProgressRingButton : Button
    {
        private string _translation;
        private bool _isLoading;

        public string Translation
        {
            get
            {
                return _translation;
            }
            set
            {
                _translation = value;

                Label.Text = ResourceLoader.GetForCurrentView().GetString(value);
            }
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                IsEnabled = !value;
                ProgressRing.IsActive = value;

                _isLoading = value;
            }
        }

        public ProgressRingButton()
        {
            InitializeComponent();
        }
    }
}
