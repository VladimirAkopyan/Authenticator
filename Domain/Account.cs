using Domain.Protocols;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Domain
{
    public class Account : INotifyPropertyChanged
    {
        private string _service;
        private byte _digits = TOTP.DEFAULT_DIGITS;
        private bool isModified;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Secret { get; set; }
        public string Username { get; set; }

        public byte Digits
        {
            get
            {
                return _digits;
            }
            set
            {
                _digits = NormalizeDigits(value);
            }
        }

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

                NotifyPropertyChanged("Service");
            }
        }

        [JsonIgnore]
        public bool IsModified
        {
            get
            {
                return isModified;
            }
        }

        [JsonConstructor]
        public Account(string username, string secret, string service, byte digits = TOTP.DEFAULT_DIGITS)
        {
            Username = username;
            Secret = secret;
            _service = service;
            Digits = digits;
        }

        public void Flush()
        {
            isModified = false;
        }

        public override bool Equals(object obj)
        {
            Account account = obj as Account;

            return account != null && account.Username == Username && account.Service == Service;
        }

        public override int GetHashCode()
        {
            int usernameHash = Username != null ? Username.GetHashCode() : 0;
            int serviceHash = Service != null ? Service.GetHashCode() : 0;

            return usernameHash ^ serviceHash;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static byte NormalizeDigits(byte digits)
        {
            if (digits < TOTP.MIN_DIGITS || digits > TOTP.MAX_DIGITS)
            {
                return TOTP.DEFAULT_DIGITS;
            }

            return digits;
        }
    }
}
