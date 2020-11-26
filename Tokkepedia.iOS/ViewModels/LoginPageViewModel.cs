using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Tokkepedia.iOS.Setups;
using Tokkepedia.iOS.ViewModels.Base;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Tokkepedia.iOS.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
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
        public UITextField TextUserBG { get; set; }
        public UITextField TextPasswordBG { get; set; }
        #endregion

        //Box View
        UIView boxView;
        UIActivityIndicatorView activitySpinner;
        UILabel lblSpinner;

        #region Constructors
        /// <summary>
        ///     Constructors will be called during the Registration in ViewModelLocator (Applying Dependency Injection or Inversion of Controls)
        /// </summary>
        public LoginPageViewModel()
        {
            Credentials = new LoginModel(); // Initialized Model to avoid nullreference exception
            // Initialize Commands here...
            LoginCommand = new RelayCommand(async () => await Login(), IsLogin);

            CreateLoadingSpinner();
        }
        #endregion

        //Box View
        void CreateLoadingSpinner()
        {
            var midX = UIScreen.MainScreen.Bounds.GetMidX();
            var midY = UIScreen.MainScreen.Bounds.GetMidY();

            //CGRectGetMidX(mainRect), CGRectGetMidY(mainRect)
            // Box config:
            boxView = new UIView() 
            { 
                Frame = new CGRect() { X = midX, Y = midY, Width = 0, Height = 0 },
                BackgroundColor = UIColor.Black,
                Alpha = 0.9f,
                Bounds = new CGRect() { X = 0, Y = 0, Width = 150, Height = 150 },
            };
            boxView.Layer.CornerRadius = 15;
            
            // Spin config:
            activitySpinner = new UIActivityIndicatorView() 
            { 
                ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge,
            };
            activitySpinner.StartAnimating();


            // Text config:
            lblSpinner = new UILabel()
            {
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                Text = "Logging in...",
                Bounds = new CGRect(x: 0, y: 0, width: 80, height: 30),
            };
            lblSpinner.Font = lblSpinner.Font.WithSize(13);

            // Activate:
            boxView.AddSubview(activitySpinner);
            boxView.AddSubview(lblSpinner);

            activitySpinner.Frame = new CGRect() { X = boxView.Bounds.GetMidX(), Y = boxView.Bounds.GetMidY(), Width = 0, Height = 0 };
            lblSpinner.Frame = new CGRect(x: boxView.Bounds.GetMidX(), y: boxView.Bounds.GetMidY(), width: 0, height: 0);

            //view.AddSubview(boxView)

        }

        #region Methods/Events
        public async Task Login()
        {
            UIApplication.SharedApplication.KeyWindow.AddSubview(boxView);

            IsLogin = true;
            if (!String.IsNullOrEmpty(Credentials.Username) && !String.IsNullOrEmpty(Credentials.Password))
            {
                var result = await AccountService.Instance.Login(Credentials);
                if (result.ResultEnum == Shared.Helpers.Result.Success)
                {
                    //TextUserBG.Background(Resource.Layout.rounded_border_edittext);
                    //TextPasswordBG.SetBackgroundResource(Resource.Layout.rounded_border_edittext);
                    Settings.UserAccount = result.ResultObject.ToString();
                    var tokens = Settings.GetUserModel();

                    //https://docs.microsoft.com/en-us/xamarin/essentials/secure-storage?context=xamarin%2Fandroid&tabs=ios
                    //When developing on the iOS simulator, enable the Keychain entitlement and add a keychain access group for the application's bundle identifier.
                    //Open the Entitlements.plist in the iOS project and find the Keychain entitlement and enable it.This will automatically add the application's identifier as a group.
                    //In the project properties, under iOS Bundle Signing set the Custom Entitlements to Entitlements.plist.
                    //When deploying to an iOS device this entitlement is not required and should be removed.
                    await SecureStorage.SetAsync("idtoken", tokens.IdToken);
                    await SecureStorage.SetAsync("refreshtoken", tokens.RefreshToken);
                    await SecureStorage.SetAsync("userid", tokens.UserId);


                    UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);
                    UITabBarController mainController = Storyboard.InstantiateViewController("MainTabController") as MainTabController;
                    //Navigation.PushViewController(mainController, false);
                    UIApplication.SharedApplication.Windows[0].RootViewController = mainController;
                }
                else
                {
                    // Handle error when saving
                    //TextUserBG.SetBackgroundResource(Resource.Drawable.error_background_text);
                    //TextPasswordBG.SetBackgroundResource(Resource.Drawable.error_background_text);
                    var dialog = ServiceLocator.Current.GetInstance<IDialogService>();
                    await
                        dialog.ShowError(
                            "Incorrect email or password.",
                            "Error",
                            "OK",
                            null);
                }
                //ProgressCircle.Visibility = ViewStates.Invisible;
                //ProgressLoadingText.Visibility = ViewStates.Invisible;
            }

            //var nav = ServiceLocator.Current.GetInstance<INavigationService>();
            //nav.GoBack();
            IsLogin = false;

            boxView.RemoveFromSuperview();
        }
        #endregion
    }
}