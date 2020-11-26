using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using ServiceAccount = Tokkepedia.Shared.Services;

namespace Tokkepedia
{
#if (_TOKKEPEDIA)
    [Activity(Label = "", Theme = "@style/CustomAppTheme")]
#endif

#if (_CLASSTOKS)
    [Activity(Label = "", Theme = "@style/CustomAppThemeBlue")]
#endif

    public class ProfileInputBox : BaseActivity
    {
        int inputtype;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.profile_page_inputbox);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            TxtInputBox.Text = Intent.GetStringExtra("inputbox");
            inputtype = Intent.GetIntExtra("inputtype",0);
            CircleProgress.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.White, Android.Graphics.PorterDuff.Mode.Multiply);

            if (inputtype == 1001) 
            {
                this.Title = "Update Bio";
            }
            else if (inputtype == 1002)
            {
                this.Title = "Update Website";
                InputLayoutInputBox.CounterEnabled = true;
                InputLayoutInputBox.CounterMaxLength = 50;
                TxtInputBox.SetFilters(new IInputFilter[] { new InputFilterLengthFilter(50) });
            }
            else if (inputtype == 1003)//Name
            {
                this.Title = "Update Display Name";
                InputLayoutInputBox.CounterEnabled = true;
                InputLayoutInputBox.CounterMaxLength = 25;
                TxtInputBox.SetFilters(new IInputFilter[] { new InputFilterLengthFilter(25) });
            }

            SaveButton.Click += async(object sender, EventArgs e) =>
            {
                CircleProgress.Visibility = ViewStates.Visible;
                this.Window.AddFlags(WindowManagerFlags.NotTouchable);
                if (inputtype == 1001) //BIO
                {
                    await ServiceAccount.AccountService.Instance.UpdateUserBioAsync(TxtInputBox.Text);
                }
                else if (inputtype == 1002) // Website
                {
                    await ServiceAccount.AccountService.Instance.UpdateUserWebsiteAsync(TxtInputBox.Text);
                }
                else if (inputtype == 1003) // Name
                {
                    await ServiceAccount.AccountService.Instance.UpdateUserDisplayNameAsync(TxtInputBox.Text);
                }
                CircleProgress.Visibility = ViewStates.Gone;
                this.Window.ClearFlags(WindowManagerFlags.NotTouchable);

                Intent = new Intent();
                Intent.PutExtra("inputbox", TxtInputBox.Text);
                SetResult(Android.App.Result.Ok, Intent);
                Finish();
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
        public EditText TxtInputBox => FindViewById<EditText>(Resource.Id.txtProfileInputBox);
        public Button SaveButton => FindViewById<Button>(Resource.Id.btnProfileInputBoxSave);
        public TextInputLayout InputLayoutInputBox => FindViewById<TextInputLayout>(Resource.Id.InputLayoutInputBox); 
        public ProgressBar CircleProgress => FindViewById<ProgressBar>(Resource.Id.circleprogressInputBox);
    }
}