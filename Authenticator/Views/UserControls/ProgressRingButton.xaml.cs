using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Authenticator_for_Windows.Views.UserControls
{
    public sealed partial class ProgressRingButton : Button
    {
        private string _translationCustom;
        private bool _isLoading;

        /// <summary>
        /// I think the original author meant localisation when he wrote translation
        /// </summary>
        public string TranslationCustom
        {
            get
            {
                return _translationCustom;
            }
            set
            {
                _translationCustom = value;

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
