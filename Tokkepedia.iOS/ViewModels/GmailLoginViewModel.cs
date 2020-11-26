//using System;

//using System.ComponentModel;
//using System.Diagnostics;
//using System.Windows.Input;
//using Plugin.GoogleClient;
//using Plugin.GoogleClient.Shared;
//using Tokkepedia.iOS.Models;
//using Xamarin.Forms;


//namespace Tokkepedia.iOS.ViewModels
//{
//    public class GmailLoginPageViewModel : INotifyPropertyChanged
//    {
//        public GmailUserProfileModel User { get; set; } = new GmailUserProfileModel();
//        public string Name
//        {
//            get => User.Name;
//            set => User.Name = value;
//        }

//        public string Email
//        {
//            get => User.Email;
//            set => User.Email = value;
//        }

//        public Uri Picture
//        {
//            get => User.Picture;
//            set => User.Picture = value;
//        }

//        public bool IsLoggedIn { get; set; }

//        public string Token { get; set; }

//        public ICommand LoginCommand { get; set; }
//        public ICommand LogoutCommand { get; set; }
//        private IGoogleClientManager _googleClientManager;
//        public event PropertyChangedEventHandler PropertyChanged;

//        public GmailLoginPageViewModel()
//        {
//            LoginCommand = new Command(LoginAsync);
//            LogoutCommand = new Command(Logout);

//            _googleClientManager = CrossGoogleClient.Current;


//            IsLoggedIn = false;
//        }

//        public async void LoginAsync()
//        {
//            _googleClientManager.OnLogin += OnLoginCompleted;
//            await _googleClientManager.LoginAsync();
//        }


//        private void OnLoginCompleted(object sender, GoogleClientResultEventArgs<GoogleUser> loginEventArgs)
//        {
//            if (loginEventArgs.Data != null)
//            {
//                GoogleUser googleUser = loginEventArgs.Data;
//                User.Name = googleUser.Name;
//                User.Email = googleUser.Email;
//                User.Picture = googleUser.Picture;
//                var GivenName = googleUser.GivenName;
//                var FamilyName = googleUser.FamilyName;


//                // Log the current User email
//                Debug.WriteLine(User.Email);
//                IsLoggedIn = true;

//                var token = CrossGoogleClient.Current.ActiveToken;
//                Token = token;
//            }
//            else
//            {
               
//            }

//            _googleClientManager.OnLogin -= OnLoginCompleted;

//        }

//        public void Logout()
//        {
//            _googleClientManager.OnLogout += OnLogoutCompleted;
//            _googleClientManager.Logout();
//        }

//        private void OnLogoutCompleted(object sender, EventArgs loginEventArgs)
//        {
//            IsLoggedIn = false;
//            User.Email = "Offline";
//            _googleClientManager.OnLogout -= OnLogoutCompleted;
//        }
//    }
//}