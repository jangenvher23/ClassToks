using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using SharedAccount = Tokkepedia.Shared.Services;

namespace Tokkepedia
{
#if (_TOKKEPEDIA)
    [Activity(Label = "Forgot Password", Theme = "@style/CustomAppTheme")]
#endif

#if (_CLASSTOKS)
    [Activity(Label = "Forgot Password", Theme = "@style/CustomAppThemeBlue")]
#endif

    public class ForgotPasswordActivity : BaseActivity
    {
        Regex EmailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.forgotpassword_page);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SubmitButton.Click += async(sender,e) =>
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, HideSoftInputFlags.None);

                ProgressBarLogin.Visibility = ViewStates.Visible;
                ProgressBarText.Visibility = ViewStates.Visible;
                if (Android.Util.Patterns.EmailAddress.Matcher(EditEmailAddress.Text.Trim()).Matches())
                {
                    var result = await SharedAccount.AccountService.Instance.SendPasswordResetAsync(EditEmailAddress.Text.Trim());
                    if (result.ResultEnum == Tokkepedia.Shared.Helpers.Result.Success)
                    {
                        ForgotPasswordConfirmation.Text = "Forgot password confirmation";
                        SubmitButton.Visibility = ViewStates.Gone;
                        EditEmailAddress.Visibility = ViewStates.Gone;
                        ForgotPasswordVerificationSent.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        EditEmailAddress.Error = result.ResultMessage;
                    }
                }
                else
                {
                    EditEmailAddress.Error = "Invalid email address";
                }
                ProgressBarLogin.Visibility = ViewStates.Gone;
                ProgressBarText.Visibility = ViewStates.Gone;
            };
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        public TextView ForgotPasswordConfirmation => FindViewById<TextView>(Resource.Id.ForgotPasswordConfirmation);
        public TextView ForgotPasswordVerificationSent => FindViewById<TextView>(Resource.Id.ForgotPasswordVerificationSent);
        public EditText EditEmailAddress => FindViewById<EditText>(Resource.Id.ForgotPasswordEditTextEmail);
        public Button SubmitButton => FindViewById<Button>(Resource.Id.ForgotPasswordBtnSubmit);

        TextView txtProgressBar; ProgressBar progressBarLogin;
        public ProgressBar ProgressBarLogin
        {
            get
            {
                return progressBarLogin
                       ?? (progressBarLogin = FindViewById<ProgressBar>(Resource.Id.progressbarLogin));
            }
        }

        public TextView ProgressBarText
        {
            get
            {
                return txtProgressBar
                       ?? (txtProgressBar = FindViewById<TextView>(Resource.Id.progressBarinsideText));
            }
        }
    }
}