using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Xamarin.Essentials;
using Tokkepedia.Shared.Helpers;
using GalaSoft.MvvmLight.Command;
using Tokkepedia.Setups;
using Tokket.Tokkepedia;
using Newtonsoft.Json;
using AndroidX.Core.Content;

namespace Tokkepedia.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        #region Properties
        public LoginModel Credentials { get; set; }
        public bool IsFacebook { get; set; }
        public bool IsGoogle { get; set; }

        private bool _isLogin = false;
        /// <summary>
        /// Sets and gets the IsSaving property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsLogin
        {
            get
            {
                return _isLogin;
            }
            set
            {
                if (Set(() => IsLogin, ref _isLogin, value))
                {
                    LoginCommand.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion

        #region Commands
        public RelayCommand LoginCommand { get; set; }
        public EditText TextUserBG { get; set; }
        public EditText TextPasswordBG { get; set; }
        public LinearLayout linearProgress { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        ///     Constructors will be called during the Registration in ViewModelLocator (Applying Dependency Injection or Inversion of Controls)
        /// </summary>
        public LoginPageViewModel()
        {
            Credentials = new LoginModel(); // Initialized Model to avoid nullreference exception
            // Initialize Commands here...
            LoginCommand = new RelayCommand(async () => await Login(), IsLogin);
        }
        #endregion

        #region Methods/Events
        public async Task Login()
        {
            IsLogin = true;
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                if (!String.IsNullOrEmpty(Credentials.Username) && !String.IsNullOrEmpty(Credentials.Password))
                {
                    linearProgress.Visibility = ViewStates.Visible;
                    var result = await AccountService.Instance.Login(Credentials);
                    if (result.ResultEnum == Shared.Helpers.Result.Success)
                    {
                        TextUserBG.SetBackgroundResource(Resource.Layout.rounded_border_edittext);
                        TextPasswordBG.SetBackgroundResource(Resource.Layout.rounded_border_edittext);
                        var resultObject = result.ResultObject.ToString();
                        var userAccount = JsonConvert.DeserializeObject<FirebaseTokenModel>(resultObject);
                        TokketUser tokketUser = await AccountService.Instance.GetUserAsync(userAccount.UserId);
                        userAccount.UserPhoto = tokketUser.UserPhoto;
                        Settings.TokketUser = JsonConvert.SerializeObject(tokketUser);
                        Settings.UserAccount = JsonConvert.SerializeObject(userAccount);
                        Settings.UserCoins = tokketUser.Coins.Value;

                        var tokens = Settings.GetUserModel();
                        await SecureStorage.SetAsync("idtoken", tokens.IdToken);
                        await SecureStorage.SetAsync("refreshtoken", tokens.RefreshToken);
                        await SecureStorage.SetAsync("userid", tokens.UserId);
                        await SecureStorage.SetAsync("accounttype", tokketUser.AccountType);

                        
                        if (tokketUser.AccountType == "group")
                        {
                            var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
                            _navigationService.NavigateTo(ViewModelLocator.SubAccountPage); // The second parameter of NavigateTo is the model or values to be passed by to the next page
                            LoginActivity.Instance.Finish();
                        }
                        else
                        {
                            var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
                            _navigationService.NavigateTo(ViewModelLocator.MainPageKey); // The second parameter of NavigateTo is the model or values to be passed by to the next page
                            LoginActivity.Instance.Finish();
                        }
                    }
                    else
                    {
                        // Handle error when saving
                        TextUserBG.SetBackgroundResource(Resource.Drawable.error_background_text);
                        TextPasswordBG.SetBackgroundResource(Resource.Drawable.error_background_text);
                        var dialog = ServiceLocator.Current.GetInstance<IDialogService>();
                        await
                            dialog.ShowError(
                                "Incorrect email and password!",
                                "Error",
                                "OK",
                                null);
                    }
                    linearProgress.Visibility = ViewStates.Invisible;
                }
            }
            else
            {
                var dialog = ServiceLocator.Current.GetInstance<IDialogService>();
                await
                    dialog.ShowError(
                        "No internet access!",
                        "Failed to connect!",
                        "OK",
                        null);
            }
            IsLogin = false;
        }
        #endregion
    }
}