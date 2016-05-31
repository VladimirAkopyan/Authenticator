using System.ComponentModel;

namespace Domain
{
    public class Account : INotifyPropertyChanged
    {
        private string _service;
        private bool isModified;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Secret { get; set; }
        public string Username { get; set; }
        public string Service
        {
            get
            {
                return _service;
            }
            set
            {
                _service = value;
                isModified = true;

                PropertyChanged(this, new PropertyChangedEventArgs("Service"));
            }
        }

        public bool IsModified
        {
            get
            {
                return isModified;
            }
        }

        public Account(string username, string secret, string service)
        {
            Username = username;
            Secret = secret;
            _service = service;
        }

        public void Flush()
        {
            isModified = false;
        }
    }
}
