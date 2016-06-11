using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Authenticator_for_Windows.Views.UserControls
{
    public sealed partial class Banner : UserControl
    {
        private BannerType _bannerType;

        public BannerType BannerType
        {
            get
            {
                return _bannerType;
            }
            set
            {
                setBannerType(value);
                _bannerType = value;
            }
        }

        public string BannerText
        {
            get
            {
                return BannerTextBlock.Text;
            }
            set
            {
                BannerTextBlock.Text = value;
            }
        }

        public bool Dismissable
        {
            get
            {
                return BannerCloseButton.Visibility == Visibility.Visible;
            }
            set
            {
                if (!value)
                {
                    BannerCloseButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        public Banner()
        {
            InitializeComponent();

            BannerType = BannerType.Info;
        }

        public Banner(BannerType bannerType, string bannertext) : this()
        {
            BannerText = bannertext;
            BannerType = bannerType;
        }

        public Banner(BannerType bannerype, string bannertext, bool dismissable) : this(bannerype, bannertext)
        {
            Dismissable = dismissable;
        }

        public void AddButton(Button button)
        {
            button.Foreground = BannerMainPanel.Background;
            button.Background = Background = BannerTextBlock.Foreground;
            button.Margin = new Thickness(10, 0, 0, 0);

            BannerStackPanel.Children.Add(button);
        }

        private void setBannerType(BannerType bannerType)
        {
            switch (bannerType)
            {
                case BannerType.Success:
                    BannerMainPanel.Background = new SolidColorBrush(Color.FromArgb(255, 223, 240, 216));
                    BannerTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 60, 118, 61));
                    BannerCloseButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 60, 118, 61));
                    BannerMainPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 60, 118, 61));
                    break;
                case BannerType.Info:
                    BannerMainPanel.Background = new SolidColorBrush(Color.FromArgb(255, 217, 237, 247));
                    BannerTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 49, 112, 143));
                    BannerCloseButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 49, 112, 143));
                    BannerMainPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 49, 112, 143));
                    break;
                case BannerType.Warning:
                    BannerMainPanel.Background = new SolidColorBrush(Color.FromArgb(255, 252, 248, 227));
                    BannerTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 138, 109, 59));
                    BannerCloseButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 138, 109, 59));
                    BannerMainPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 138, 109, 59));
                    break;
                case BannerType.Danger:
                    BannerMainPanel.Background = new SolidColorBrush(Color.FromArgb(255, 242, 222, 222));
                    BannerTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 169, 68, 66));
                    BannerCloseButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 169, 68, 66));
                    BannerMainPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 169, 68, 66));
                    break;
            }
        }

        public void Close()
        {
            Panel parentPanel = (Panel)Parent;
            parentPanel.Children.Remove(this);
        }

        private void BannerCloseButton_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Close();
        }
    }
}