// This file has been autogenerated from a class added in the UI designer.

using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows.Input;
using Foundation;
using GalaSoft.MvvmLight.Helpers;
using Newtonsoft.Json;
using Tokkepedia.iOS.ViewModels;
using Tokkepedia.Shared.Models;
using UIKit;
using Xamarin.Essentials;
using SharedHelpers = Tokkepedia.Shared.Helpers;
using SharedService = Tokkepedia.Shared.Services;
using Xamarin.Auth;
using System.Json;
using Plugin.GoogleClient;
using Plugin.GoogleClient.Shared;

namespace Tokkepedia.iOS
{
    public partial class LoginController : UITableViewController
    {
        //Controls
        UITextField _userName, _password;
        UIButton btnLogin;

        UITabBarController mainController;

        public LoginPageViewModel LoginVm => App.Locator.LoginPageVM;
        private Binding<string, string> _usernameBinding, _passwordBinding;


        private bool CheckCredentials()
        {
            bool resultbool = false;
            var idtoken = SecureStorage.GetAsync("idtoken").GetAwaiter().GetResult();
            var refreshtoken = SecureStorage.GetAsync("refreshtoken").GetAwaiter().GetResult();
            var userid = SecureStorage.GetAsync("userid").GetAwaiter().GetResult();
            var useraccount = JsonConvert.DeserializeObject<FirebaseTokenModel>(SharedHelpers.Settings.UserAccount);
            if (idtoken != null && refreshtoken != null && userid != null && useraccount != null)
            {
                useraccount.UserId = userid;
                var result = SharedService.AccountService.Instance.VerifyToken(idtoken, refreshtoken);
                if (result != null)
                {
                    if (result.ResultEnum == SharedHelpers.Result.Success)
                    {
                        resultbool = true;
                    }
                }
            }

            return resultbool;
        }

        public class LinkerWorkarounds
        {
            public void Include(UITextField textField)
            {
                textField.Text = textField.Text + "";
                textField.EditingChanged += (sender, args) => { textField.Text = ""; };
            }

            public void Include(UIButton uiButton)
            {
                uiButton.TouchUpInside += (sender, e) =>
                {
                    uiButton.SetTitle(uiButton.Title(UIControlState.Normal), UIControlState.Normal);
                    ////Create Alert
                    //var okAlertController = UIAlertController.Create("Title", "The message", UIAlertControllerStyle.Alert);

                    ////Add Action
                    //okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

                    //// Present Alert
                    //UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(okAlertController, true, null);
                };
            }
        }


        public LoginController(IntPtr handle) : base(handle)
        {

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            btnFacebook.TouchUpInside += btnFacebook_TouchUpInside;
            btnGmail.TouchUpInside += btnGmail_TouchUpInside;

            this.TableView.BackgroundView = new UIImageView()
            {
                Image = UIImage.FromBundle("bg1.png")
            };


            LoginVm.Navigation = this.NavigationController;
            UIApplication.SharedApplication.SetStatusBarHidden(true, true);

            //hides keypad when not focused on textfield
            var g = new UITapGestureRecognizer(() => View.EndEditing(true));
            g.CancelsTouchesInView = false; //for iOS5   
            View.AddGestureRecognizer(g);

            //Perform any additional setup after loading the view, typically from a nib.
            if (CheckCredentials())
            {
                mainController = Storyboard.InstantiateViewController("MainTabController") as MainTabController;
                UIApplication.SharedApplication.Windows[0].RootViewController = mainController; 
            }
            else
            {
                // Retrieve navigation parameter and set as current "DataContext"
                _usernameBinding = this.SetBinding(() => LoginVm.Credentials.Username, () => Username.Text, BindingMode.TwoWay);
                _passwordBinding = this.SetBinding(() => LoginVm.Credentials.Password, () => Password.Text, BindingMode.TwoWay);

                LoginVm.TextUserBG = Username;
                LoginVm.TextPasswordBG = Password;
                //LoginVm.ProgressCircle = ProgressBarLogin;
                //LoginVm.ProgressLoadingText = ProgressBarText;
                BtnLogin.SetCommand("TouchUpInside", LoginVm.LoginCommand);
            }
        }

        #region Facebook and Google
        private void btnFacebook_TouchUpInside(object sender, EventArgs e)
        {
            var auth = new OAuth2Authenticator("2096904330562973", "email", 
                new Uri("https://m.facebook.com/dialog/oauth/"),
                new Uri("https://m.facebook.com/connect/login_success.html"));
                            
            auth.Completed += Auth_FbCompleted;
                       
            var ui = auth.GetUI();         
            PresentViewController(ui, true, null);

        }

        private async void Auth_FbCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            if (e.IsAuthenticated)
            {
                var request = new OAuth2Request("GET",
                    new Uri("https://graph.facebook.com/me?fields=name"),
                    null,
                    e.Account);

                var fbResponse = await request.GetResponseAsync();
                var fbUser = JsonValue.Parse(fbResponse.GetResponseText());
                var name = fbUser["name"];


                //string emailAddress = string.Empty;

                //var token = e.Account.Properties["access_token"].ToString();
                //var expiresIn = Convert.ToDouble(e.Account.Properties["expires_in"]);
                //var expireDate = DateTime.Now + TimeSpan.FromSeconds(expiresIn);
                //var request = new OAuth2Request("GET", new Uri("https://www.googleapis.com/auth/userinfo"), null, e.Account);
                //var response = await request.GetResponseAsync();
                //var obj = JObject.Parse(response.GetResponseText());
                //var id = obj["id"].ToString().Replace("\"", "");
                //var name = obj["name"].ToString().Replace("\"", "");


                //mainController = Storyboard.InstantiateViewController("MainTabController") as MainTabController;
                //UIApplication.SharedApplication.Windows[0].RootViewController = mainController;
            }
            DismissViewController(true, null);
        }


        private async void btnGmail_TouchUpInside(object sender, EventArgs e)
        {
            //var auth = new OAuth2Authenticator("1021534238990-kfgvd3lqdfautku1aeej645ip6lsiogs.apps.googleusercontent.com",
            //    "https://www.googleapis.com/auth/userinfo.email",
            //    new Uri("https://accounts.google.com/o/oauth2/auth"),
            //    new Uri("https://www.googleapis.com/plus/v1/people/me:/oauth2redirect"));

            //auth.Completed += Auth_FbCompleted;

            //var ui = auth.GetUI();
            //PresentViewController(ui, true, null);

            IGoogleClientManager _googleClientManager =  CrossGoogleClient.Current;   
            _googleClientManager.OnLogin += OnLoginCompleted;
            await _googleClientManager.LoginAsync();

        }
        #endregion


        private void OnLoginCompleted(object sender, GoogleClientResultEventArgs<GoogleUser> loginEventArgs)
        {
            if (loginEventArgs.Data != null)
            {
                
              
            }
            else
            {
                
            }
        }

        //private async void Auth_GmailCompleted(object sender, AuthenticatorCompletedEventArgs e)
        //{
        //    //Fires when authentication is cancelled
        //    if (!e.IsAuthenticated)
        //    {
        //        //Authentication failed Do something
        //        return;
        //    }
        //    //Make request to get the parameters access
        //    var request = new OAuth2Request
        //                  (
        //                     "GET",
        //                      new Uri("https://www.googleapis.com/oauth2/v2/userinfo"),
        //                      null,
        //                      e.Account
        //                  );
        //    //Get response here
        //    var response = await request.GetResponseAsync();
        //    if (response != null)
        //    {
        //        //Get the user data here
        //        var userData = response.GetResponseText();
        //    }
        //}

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            
        }


        public UITextField Username
        {
            get
            {
                return _userName ?? (_userName = txtEmail);
            }
        }
        public UITextField Password
        {
            get
            {
                return _password ?? (_password = txtPassword);
            }
        }
        public UIButton BtnLogin
        {
            get
            {
                return btnLogin ?? (btnLogin = btnTokketLogin);
            }
        }
    }
}
