using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Domain;
using Authenticator.Events;
using Windows.ApplicationModel.DataTransfer;
using Settings;
using Domain.Utilities;
using Domain.Storage;
using Synchronization.Exceptions;
using Authenticator.Views.Pages;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;

namespace Authenticator.Views.UserControls
{
    public sealed partial class AccountBlock : UserControl
    {
        private Account account;
        private OTP otp;
        private bool _inEditMode;
        private DispatcherTimer timer;
        private MainPage mainPage;
        
        private bool skipEditModeAnimation;

        public bool InEditMode
        {
            get
            {
                return _inEditMode;
            }
            set
            {
                _inEditMode = value;

                Storyboard storyBoard = null;

                if (value)
                {
                    storyBoard = Open;
                }
                else
                {
                    storyBoard = Close;
                }

                storyBoard.Begin();

                if (skipEditModeAnimation)
                {
                    storyBoard.SkipToFill();
                }
            }
        }

        private bool _editEnabled;

        public bool EditEnabled
        {
            get
            {
                return _editEnabled;
            }
            set
            {
                _editEnabled = value;

                Delete.IsEnabled = value;
            }
        }

        public AccountBlock()
        {

        }

        public AccountBlock(Account account, bool flash, MainPage mainPage)
        {
            Initialize(account, flash, mainPage);
        }

        public AccountBlock(Account account, MainPage mainPage)
        {
            Initialize(account, false, mainPage);
        }

        public void Update()
        {
            if (account != null)
            {
                FadeOut.Begin();
            }
        }

        private void Initialize(Account account, bool flash, MainPage mainPage)
        {
            InitializeComponent();

            this.account = account;
            this.mainPage = mainPage;
            otp = new OTP(account.Secret, account.Digits);

            DataContext = account;

            DisplayCodeFormatted();

            if (flash)
            {
                Flash.Begin();
            }
        }

        private void SlideUp_Completed(object sender, object e)
        {
            NotifyRemoved();
        }

        private void DisplayCodeFormatted()
        {
            CurrentCode.Text = FormatCode(otp.Code);
        }

        private static string FormatCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length <= 4)
            {
                return code;
            }

            int groupSize = code.Length % 3 == 0 ? 3 : 4;
            List<string> groups = new List<string>();

            for (int index = 0; index < code.Length; index += groupSize)
            {
                int remaining = code.Length - index;
                groups.Add(code.Substring(index, Math.Min(groupSize, remaining)));
            }

            return string.Join(" ", groups);
        }

        public void Remove()
        {
            SlideUp.Begin();
        }

        public void Show(bool inEditMode)
        {
            skipEditModeAnimation = !inEditMode;

            SlideDown.Begin();
        }

        private void FadeOut_Completed(object sender, object e)
        {
            DisplayCodeFormatted();

            FadeIn.Begin();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            NotifyDeleteRequested();
        }

        public event EventHandler<DeleteRequestEventArgs> DeleteRequested;
        public event EventHandler<EventArgs> Removed;
        public event EventHandler<EventArgs> CopyRequested;
        public event EventHandler<EventArgs> Modified;

        private void NotifyDeleteRequested()
        {
            if (DeleteRequested != null)
            {
                DeleteRequested(this, new DeleteRequestEventArgs(account));
            }
        }

        private void NotifyRemoved()
        {
            if (Removed != null)
            {
                Removed(this, new EventArgs());
            }
        }

        private void NotifyCopyRequested()
        {
            if (CopyRequested != null)
            {
                CopyRequested(this, new EventArgs());
            }
        }

        private void NotifyModified()
        {
            if (Modified != null)
            {
                Modified(account, new EventArgs());
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!InEditMode)
            {
                CopyCodeAndNotify();
            }
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (InEditMode)
            {
                return;
            }

            CoreVirtualKeyStates controlKeyState = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            bool controlDown = (controlKeyState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            bool copyShortcut = controlDown && (e.Key == VirtualKey.C || e.Key == VirtualKey.Insert);
            bool activationKey = e.Key == VirtualKey.Enter || e.Key == VirtualKey.Space;

            if (copyShortcut || activationKey)
            {
                CopyCodeAndNotify();
                e.Handled = true;
            }
        }

        private void CopyCodeAndNotify()
        {
            if (Flash.GetCurrentState() != ClockState.Stopped)
            {
                Flash.Stop();
            }

            Flash.Begin();

            CopyCode();
            NotifyCopyRequested();
        }

        private void CopyCode()
        {
            int clipboardType = SettingsManager.Get<int>(Setting.ClipBoardRememberType);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(otp.Code);
            Clipboard.SetContent(dataPackage);

            // Type 1 = dynamic, type 2 = forever

            if (clipboardType == 0)
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Interval = new TimeSpan(0, 0, TOTPUtilities.RemainingSeconds);
                }
                else
                {
                    timer = new DispatcherTimer()
                    {
                        Interval = new TimeSpan(0, 0, TOTPUtilities.RemainingSeconds)
                    };

                    timer.Tick += Timer_Tick;
                }

                timer.Start();
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            try
            {
                Clipboard.Clear();
            }
            catch (Exception)
            {
                // Cannot clear the clipboard (perhaps it's in use)
            }

            timer.Stop();
        }

        private void Open_Completed(object sender, object e)
        {
            ShakePencil.Begin();

            if (skipEditModeAnimation)
            {
                ShakePencil.SkipToFill();

                skipEditModeAnimation = false;
            }
        }

        private async void EditPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_editEnabled && InEditMode)
            {
                ModifyServiceDialog dialog = new ModifyServiceDialog(account);
                await dialog.ShowAsync();

                if (dialog.IsModified)
                {
                    NotifyModified();
                }
            }
        }
    }
}
