using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Authenticator_for_Windows.Views.UserControls
{
    public sealed partial class CodeCopiedNotification : UserControl
    {
        public CodeCopiedNotification()
        {
            InitializeComponent();
        }

        public void Animate()
        {
            CopiedOpenClose.Begin();
        }
    }
}
