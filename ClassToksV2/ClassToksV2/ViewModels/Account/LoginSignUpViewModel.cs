using ClassToksV2.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ClassToksV2.ViewModels
{
    public class LoginSignUpViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }
        public Command SignUpCommand { get; }

        public Command GoToLoginCommand { get; }
        public Command GoToSignUpCommand { get; }

        public Command TermsCommand { get; }

        public WaitingView Loader { get; set; }

        private string email = "tokketapp@gmail.com";
        public string Email
        {
            get { return email; }
            set { SetProperty(ref email, value); }
        }

        private string password = "tokketapp";
        public string Password
        {
            get { return password; }
            set { SetProperty(ref password, value); }
        }

        public TokketUser User { get; set; } = new TokketUser();

        private bool isSignUpMode = false;
        public bool IsSignUpMode
        {
            get { return isSignUpMode; }
            set { SetProperty(ref isSignUpMode, value); }
        }

        private bool isLoginMode = true;
        public bool IsLoginMode
        {
            get { return isLoginMode; }
            set { SetProperty(ref isLoginMode, value); }
        }

        private bool isLoading = false;
        public bool IsLoading
        {
            get { return isLoading; }
            set { SetProperty(ref isLoading, value); }
        }

        public LoginSignUpViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            SignUpCommand = new Command(OnSignUpClicked);
            GoToLoginCommand = new Command(OnGoToLoginClicked);
            GoToSignUpCommand = new Command(OnGoToSignUpClicked);
            TermsCommand = new Command(OnTermsClicked);
        }

        private async void OnLoginClicked(object obj)
        {
            IsLoading = false;
            if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password))
            {
                //Loader.IsVisible = true;
                IsLoading = true;

                LoginModel Credentials = new LoginModel() { Username = Email, Password = Password };
                var result = await AccountService.Instance.Login(Credentials);
                if (result.ResultEnum == Tokkepedia.Shared.Helpers.Result.Success)
                {
                    var resultObject = result.ResultObject.ToString();
                    var userAccount = JsonConvert.DeserializeObject<FirebaseTokenModel>(resultObject);
                    TokketUser tokketUser = await AccountService.Instance.GetUserAsync(userAccount.UserId);
                    userAccount.UserPhoto = tokketUser.UserPhoto;
                    Settings.TokketUser = JsonConvert.SerializeObject(tokketUser);
                    Settings.UserAccount = JsonConvert.SerializeObject(userAccount);
                    Settings.UserCoins = tokketUser.Coins.Value;

                    #region App Shell UI
                    Application.Current.MainPage = App.AppShellInstance = new AppShell();
                    Shell.SetTabBarIsVisible(Shell.Current, true);

                    if (string.IsNullOrEmpty(tokketUser.CoverPhoto))
                    {
                        App.AppShellInstance.FlyoutCoverPhoto.IsVisible = false;
                    }
                    else
                    {
                        App.AppShellInstance.FlyoutCoverPhoto.IsVisible = true;
                        App.AppShellInstance.FlyoutCoverPhoto.Source = tokketUser.CoverPhoto;
                    }

                    App.AppShellInstance.FlyoutUserPhoto.Source = tokketUser.UserPhoto;
                    App.AppShellInstance.FlyoutUserDisplayName.Text = tokketUser.DisplayName;
                    App.AppShellInstance.FlyoutUserTitleSubaccount.Text = tokketUser?.TitleId ?? "";

                    //await Shell.Current.GoToAsync($"..");
                    #endregion


                    var tokens = Settings.GetUserModel();
                    await SecureStorage.SetAsync("idtoken", tokens.IdToken);
                    await SecureStorage.SetAsync("refreshtoken", tokens.RefreshToken);
                    await SecureStorage.SetAsync("userid", tokens.UserId);
                    await SecureStorage.SetAsync("accounttype", tokketUser.AccountType);


                    //if (tokketUser.AccountType == "group")
                    //{
                    //    var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
                    //    _navigationService.NavigateTo(ViewModelLocator.SubAccountPage); // The second parameter of NavigateTo is the model or values to be passed by to the next page
                    //    LoginActivity.Instance.Finish();
                    //}
                    //else
                    //{
                    //    var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
                    //    _navigationService.NavigateTo(ViewModelLocator.MainPageKey); // The second parameter of NavigateTo is the model or values to be passed by to the next page
                    //    LoginActivity.Instance.Finish();
                    //}
                }
                else
                {
                    // Handle error when saving
                    //await Application.Current.MainPage.DisplayAlert("Error", "Incorrect email and password!", "Ok");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Notice", "Please enter your email and password.", "OK");
            }
            IsLoading = false;
            //Loader.IsVisible = false;


            //await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
        }

        private async void OnSignUpClicked(object obj)
        {

        }

        private async void OnGoToLoginClicked(object obj)
        {
            IsSignUpMode = false;
            IsLoginMode = true;
        }

        private async void OnGoToSignUpClicked(object obj)
        {
            IsSignUpMode = true;
            IsLoginMode = false;
        }

        private async void OnTermsClicked(object obj)
        {
            IsSignUpMode = false;
            IsLoginMode = true;
        }
    }
}
