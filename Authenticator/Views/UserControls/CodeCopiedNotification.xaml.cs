using Windows.UI.Xaml.Controls;

namespace Authenticator.Views.UserControls
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
