using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Text;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.Content;
using GalaSoft.MvvmLight.Helpers;
using GalaSoft.MvvmLight.Views;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Tokkepedia.Shared.Models;
using Tokkepedia.ViewModels;
using Tokket.Tokkepedia;
using Xamarin.Essentials;
using NetworkAccess = Xamarin.Essentials.NetworkAccess;
using SharedAccount = Tokkepedia.Shared.Services;
using SharedHelpers = Tokkepedia.Shared.Helpers;

namespace Tokkepedia
{
    [Activity(Label = "Login Activity", Theme = "@style/AppTheme")]

    public class LoginActivity : ActivityBase //, IFacebookCallback , GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
        //Facebook
       /* private ICallbackManager mFBCallManager;
        private MyProfileTracker mProfileTracker;*/

        //Google
        /*GoogleApiClient mGoogleApiClient;
        private ConnectionResult mConnectionResult;
        SignInButton btnGoogleLogin;
        private bool mIntentInProgress;
        private bool mSignInClicked;
        private bool mInfoPopulated;*/

        //Controls
        EditText _userName, _password;
        Button btnLogin;
        TextView txtProgressBar; ProgressBar progressBarLogin;

        Intent nextActivity = null;
        public string TAG
        {
            get;
            private set;
        }
        FirebaseTokenModel Useraccount;
        internal static LoginActivity Instance { get; private set; }
        public LoginPageViewModel LoginVm => App.Locator.LoginPageVM;
        private Binding<string, string> _usernameBinding, _passwordBinding;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.login_page);

            SharedHelpers.Settings.ActivityInt = 0;

            TextView txtSignUp = FindViewById<TextView>(Resource.Id.txtSignup);
            //LoginButton btnFBLogin = FindViewById<LoginButton>(Resource.Id.btnFBLogin);
            var txtErrorPassword = FindViewById<TextInputLayout>(Resource.Id.txtLoginErrorPassword);
            //btnGoogleLogin = FindViewById<SignInButton>(Resource.Id.btnGoogleLogin);

            Instance = this;

            /*//Facebook
            mProfileTracker = new MyProfileTracker();
            mProfileTracker.mOnProfileChanged += mProfileTracker_mOnProfileChanged;
            mProfileTracker.StartTracking();

            //Facebook
            btnFBLogin.SetPermissions(new List<string> {
                    "user_friends",
                    "public_profile"
                     });
            mFBCallManager = CallbackManagerFactory.Create();
            btnFBLogin.RegisterCallback(mFBCallManager, this);*/

            //Google
            /*btnGoogleLogin.Click += btnGoogleLogin_Click;
            GoogleApiClient.Builder builder = new GoogleApiClient.Builder(this);
            builder.AddConnectionCallbacks(this);
            builder.AddOnConnectionFailedListener(this);
            builder.AddApi(PlusClass.API);
            builder.AddScope(PlusClass.ScopePlusProfile);
            builder.AddScope(PlusClass.ScopePlusLogin);
            //Build our IGoogleApiClient  
            mGoogleApiClient = builder.Build();*/

            txtSignUp.Click += (object sender, EventArgs e) =>
            {
                nextActivity = new Intent(this, typeof(SignupActivity));
                StartActivity(nextActivity);
            };

            LabelForgotPassword.Click += delegate
            {
                nextActivity = new Intent(this, typeof(ForgotPasswordActivity));
                StartActivity(nextActivity);
            };

#if (_CLASSTOKS)
            BtnLogin.SetBackgroundResource(Resource.Drawable.blue_button);
            relativeLayoutParent.SetBackgroundResource(Resource.Drawable.bg_classtok);
            LogoText.SetImageResource(Resource.Drawable.classtok_text);
            ProgressBarLogin.IndeterminateDrawable.SetColorFilter(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)), Android.Graphics.PorterDuff.Mode.Multiply);
#endif

            if (CheckCredentials())
            {
                var current = Connectivity.NetworkAccess;

                if (current == NetworkAccess.Internet)
                {
                    nextActivity = new Intent(this, typeof(MainActivity));
                    StartActivity(nextActivity);
                    Finish();
                }
                else
                {
                    var dialognetwork = new Android.App.AlertDialog.Builder(this);
                    var alertnetwork = dialognetwork.Create();
                    alertnetwork.SetTitle("");
                    alertnetwork.SetMessage("No internet access.");
                    alertnetwork.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                    alertnetwork.Show();
                    alertnetwork.SetCanceledOnTouchOutside(false);
                }
            }
            else
            {
                // Retrieve navigation parameter and set as current "DataContext"
                _usernameBinding = this.SetBinding(() => LoginVm.Credentials.Username, () => Username.Text, BindingMode.TwoWay);
                _passwordBinding = this.SetBinding(() => LoginVm.Credentials.Password, () => Password.Text, BindingMode.TwoWay);

                LoginVm.TextUserBG = Username;
                LoginVm.TextPasswordBG = Password;
                LoginVm.linearProgress = linearProgress;
                BtnLogin.SetCommand(
                "Click", LoginVm.LoginCommand);
            }

            BtnLogin.Click += delegate
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager.HideSoftInputFromWindow(BtnLogin.WindowToken, HideSoftInputFlags.None);
            };

        }
        private bool CheckCredentials()
        {
            bool resultbool = false;

            var idtoken = SecureStorage.GetAsync("idtoken").GetAwaiter().GetResult();
            var refreshtoken = SecureStorage.GetAsync("refreshtoken").GetAwaiter().GetResult();
            var userid = SecureStorage.GetAsync("userid").GetAwaiter().GetResult();

            Useraccount = JsonConvert.DeserializeObject<FirebaseTokenModel>(SharedHelpers.Settings.UserAccount);
            
            if (idtoken != null && refreshtoken != null && userid != null && Useraccount != null)
            {
                Useraccount.UserId = userid;
                var result = SharedAccount.AccountService.Instance.VerifyToken(idtoken, refreshtoken);
                if (result != null)
                {
                    if (result.ResultEnum == SharedHelpers.Result.Success)
                    {
                        if (SharedHelpers.Settings.GetTokketUser() == null)
                        {
                            this.RunOnUiThread(async () => await LoadTokketUser());
                        }
                        
                        if (SharedHelpers.Settings.GetTokketUser().AccountType == "group")
                        {
                            if (SharedHelpers.Settings.GetTokketSubaccount() == null)
                            {
                                Intent nextActivity = new Intent(this, typeof(SubAccountActivity));
                                Finish();
                                StartActivity(nextActivity);
                            }
                        }
                        else
                        {
                            resultbool = true;
                        }                        
                    }
                }
            }
            return resultbool;
        }
        private async Task LoadTokketUser()
        {
            TokketUser tokketUser = await SharedAccount.AccountService.Instance.GetUserAsync(Useraccount.UserId);
            SharedHelpers.Settings.TokketUser = JsonConvert.SerializeObject(tokketUser);
        }
        public EditText Username
        {
            get
            {
                return _userName
                       ?? (_userName = FindViewById<EditText>(Resource.Id.txtEmail));
            }
        }
        public EditText Password
        {
            get
            {
                return _password
                       ?? (_password = FindViewById<EditText>(Resource.Id.txtPassword));
            }
        }
        public Button BtnLogin
        {
            get
            {
                return btnLogin
                       ?? (btnLogin = FindViewById<Button>(Resource.Id.btnLogin));
            }
        }

        public LinearLayout linearProgress => FindViewById<LinearLayout>(Resource.Id.linearProgress);
        public ProgressBar ProgressBarLogin => FindViewById<ProgressBar>(Resource.Id.ProgressBarLogin);
        public TextView LabelForgotPassword => FindViewById<TextView>(Resource.Id.txtForgotLogin);

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /*public void OnCancel() { }
        public void OnError(FacebookException p0) { }
        public void OnSuccess(Java.Lang.Object p0)
        {
        }
        void mProfileTracker_mOnProfileChanged(object sender, OnProfileChangedEventArgs e)
        {
            if (e.mProfile != null)
            {
                try
                {
                    //TxtFirstName.Text = e.mProfile.FirstName;
                    //TxtLastName.Text = e.mProfile.LastName;
                    //TxtName.Text = e.mProfile.Name;
                    //mprofile.ProfileId = e.mProfile.Id;
                }
                catch (Java.Lang.Exception ex) { }
            }
            else
            {
                //TxtFirstName.Text = "First Name";
                //TxtLastName.Text = "Last Name";
                //TxtName.Text = "Name";
                //mprofile.ProfileId = null;
            }
        }*/
       /* protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            mFBCallManager.OnActivityResult(requestCode, (int)resultCode, data);

            Log.Debug(TAG, "onActivityResult:" + requestCode + ":" + resultCode + ":" + data);
            if (requestCode == 0)
            {
                if (resultCode != Result.Ok)
                {
                    mSignInClicked = false;
                }
                mIntentInProgress = false;
                if (!mGoogleApiClient.IsConnecting)
                {
                    mGoogleApiClient.Connect();
                }
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            mGoogleApiClient.Connect();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (mGoogleApiClient.IsConnected)
            {
                mGoogleApiClient.Disconnect();
            }
        }
        public void OnConnected(Bundle connectionHint)
        {
            var person = PlusClass.PeopleApi.GetCurrentPerson(mGoogleApiClient);
            var name = string.Empty;
            if (person != null)
            {
                //TxtName.Text = person.DisplayName;
                //TxtGender.Text = person.Nickname;
                //var Img = person.Image.Url;
                //var imageBitmap = GetImageBitmapFromUrl(Img.Remove(Img.Length - 5));
                //if (imageBitmap != null) ImgProfile.SetImageBitmap(imageBitmap);
            }
        }

        private void btnGoogleLogin_Click(object sender, EventArgs e)
        {
            if (!mGoogleApiClient.IsConnecting)
            {
                mSignInClicked = true;
                ResolveSignInError();
            }
            else if (mGoogleApiClient.IsConnected)
            {
                PlusClass.AccountApi.ClearDefaultAccount(mGoogleApiClient);
                mGoogleApiClient.Disconnect();
            }
        }
        private void ResolveSignInError()
        {
            if (mGoogleApiClient.IsConnecting)
            {
                return;
            }
            if (mConnectionResult.HasResolution)
            {
                try
                {
                    mIntentInProgress = true;
                    StartIntentSenderForResult(mConnectionResult.Resolution.IntentSender, 0, null, 0, 0, 0);
                }
                catch (Android.Content.IntentSender.SendIntentException io)
                {
                    mIntentInProgress = false;
                    mGoogleApiClient.Connect();
                }
            }
        }
        //private Bitmap GetImageBitmapFromUrl(System.String url)
        //{
        //    Bitmap imageBitmap = null;
        //    try
        //    {
        //        using (var webClient = new WebClient())
        //        {
        //            var imageBytes = webClient.DownloadData(url);
        //            if (imageBytes != null && imageBytes.Length > 0)
        //            {
        //                imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
        //            }
        //        }
        //        return imageBitmap;
        //    }
        //    catch (IOException e) { }
        //    return null;
        //}
        public void OnConnectionFailed(ConnectionResult result)
        {
            if (!mIntentInProgress)
            {
                mConnectionResult = result;
                if (mSignInClicked)
                {
                    ResolveSignInError();
                }
            }
        }*/
        public void OnConnectionSuspended(int cause) { }
        private RelativeLayout relativeLayoutParent => FindViewById<RelativeLayout>(Resource.Id.relativeLayoutParent);
        private ImageView LogoText => FindViewById<ImageView>(Resource.Id.imageView1);
    }
}