using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Controls;

namespace Authenticator.Views.UserControls
{
    public sealed partial class UserKeyTextBox : TextBox
    {
        public bool ContainsValidUserKey { get; private set; }

        public UserKeyTextBox()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string key = new string(Text.Where(c => char.IsLetterOrDigit(c)).ToArray());
            string formattedKey = key;
            int selection = SelectionStart;

            ContainsValidUserKey = false;

            if (key.Length == 20)
            {
                formattedKey = string.Format("{0}-{1}-{2}-{3}", key.Substring(0, 5), key.Substring(5, 5), key.Substring(10, 5), key.Substring(15, 5));

                ContainsValidUserKey = true;
            }
            else if (key.Length >= 16)
            {
                if (key.Length == 16)
                {
                    selection++;
                }

                string rest = key.Substring(15, key.Length % 5);
                formattedKey = string.Format("{0}-{1}-{2}-{3}", key.Substring(0, 5), key.Substring(5, 5), key.Substring(10, 5), rest);
            }
            else if (key.Length == 15)
            {
                formattedKey = string.Format("{0}-{1}-{2}", key.Substring(0, 5), key.Substring(5, 5), key.Substring(10, 5));
            }
            else if (key.Length >= 11)
            {
                if (key.Length == 11)
                {
                    selection++;
                }

                string rest = key.Substring(10, key.Length % 5);
                formattedKey = string.Format("{0}-{1}-{2}", key.Substring(0, 5), key.Substring(5, 5), rest);
            }
            else if (key.Length == 10)
            {
                formattedKey = string.Format("{0}-{1}", key.Substring(0, 5), key.Substring(5, 5));
            }
            else if (key.Length >= 6)
            {
                if (key.Length == 6)
                {
                    selection++;
                }

                string rest = key.Substring(5, key.Length % 5);
                formattedKey = string.Format("{0}-{1}", key.Substring(0, 5), rest);
            }
            else
            {
                formattedKey = key;
            }

            formattedKey = formattedKey.ToUpper();

            Text = formattedKey;
            SelectionStart = selection;
        }
    }
}
