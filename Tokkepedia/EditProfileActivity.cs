using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using ServiceAccount = Tokkepedia.Shared.Services;

namespace Tokkepedia
{
#if (_TOKKEPEDIA)
    [Activity(Label = "Edit Profile", Theme = "@style/CustomAppTheme")]
#endif

#if (_CLASSTOKS)
    [Activity(Label = "Edit Profile", Theme = "@style/CustomAppThemeBlue")]
#endif
    public class EditProfileActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_edit_profile);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            txtBio.Text = Intent.GetStringExtra("bio");
            txtWeb.Text = Intent.GetStringExtra("web");
            txtDisplayname.Text = Intent.GetStringExtra("displayname");

            bool result = false;
            btnBio.Click += async(o, e) =>
            {
                showLoading();
                result = await ServiceAccount.AccountService.Instance.UpdateUserBioAsync(txtBio.Text);
                hideLoading();
                if (result)
                {
                    showDialog("Bio updated");
                }
            };

            btnWeb.Click += async (o, e) =>
            {
                showLoading();
                result = await ServiceAccount.AccountService.Instance.UpdateUserWebsiteAsync(txtWeb.Text);
                hideLoading();
                if (result)
                {
                    showDialog("Web updated");
                }
            };

            btnUpdateDisplayName.Click += async (o, e) =>
            {
                showLoading();
                result = await ServiceAccount.AccountService.Instance.UpdateUserDisplayNameAsync(txtDisplayname.Text);
                hideLoading();
                if (result)
                {
                    showDialog("Display name updated");
                }
            };
        }
        private void showLoading()
        {
            linearProgress.Visibility = ViewStates.Visible;
            this.Window.AddFlags(WindowManagerFlags.NotTouchable);
        }
        private void hideLoading()
        {
            linearProgress.Visibility = ViewStates.Gone;
            this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
        }

        private void showDialog(string message = "")
        {
            var dialog = new AlertDialog.Builder(this);
            var alertDialog = dialog.Create();
            alertDialog.SetTitle("");
            alertDialog.SetIcon(Resource.Drawable.alert_icon_blue);
            alertDialog.SetMessage(message);
            alertDialog.SetButton((int)(DialogButtonType.Positive), Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) =>
            {
            });
            alertDialog.Show();
            alertDialog.SetCanceledOnTouchOutside(false);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    setResult();
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        public override void OnBackPressed()
        {
            setResult();
            base.OnBackPressed();
        }

        private void setResult()
        {
            Intent = new Intent();
            Intent.PutExtra("bio", txtBio.Text);
            Intent.PutExtra("web", txtWeb.Text);
            Intent.PutExtra("displayname", txtDisplayname.Text);
            SetResult(Result.Ok, Intent);
        }

        public LinearLayout linearProgress => FindViewById<LinearLayout>(Resource.Id.linearProgress);
        public EditText txtBio => FindViewById<EditText>(Resource.Id.txtBio);
        public Button btnBio => FindViewById<Button>(Resource.Id.btnBio);
        public EditText txtWeb => FindViewById<EditText>(Resource.Id.txtWeb);
        public Button btnWeb => FindViewById<Button>(Resource.Id.btnWeb);
        public EditText txtDisplayname => FindViewById<EditText>(Resource.Id.txtDisplayname);
        public Button btnUpdateDisplayName => FindViewById<Button>(Resource.Id.btnUpdateDisplayName);
    }
}