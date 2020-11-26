using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Tokkepedia.Fragments;
using Tokkepedia.Shared.Helpers;
using SharedService = Tokkepedia.Shared.Services;
using Tokket.Tokkepedia.Tools;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Support.Design.Widget;
using Android.Icu.Util;
using Xamarin.Essentials;
using System.Drawing;
using Android.Graphics;
using Android.Webkit;

namespace Tokkepedia
{
#if (_TOKKEPEDIA)
    [Activity(Label = "Sign Up", Theme = "@style/CustomAppTheme")]
#endif

#if (_CLASSTOKS)
    [Activity(Label = "Sign Up", Theme = "@style/CustomAppThemeBlue")]
#endif

    public class SignupActivity : BaseActivity
    {
        internal static SignupActivity Instance { get; private set; }
        TextView txtBday;Spinner txtCountry; Spinner txtState; TextView txtTermsofService;
        EditText txtDisplayName;EditText txtPassword; EditText txtConfirmPassword;
        Button btnSignup; TextInputLayout txtSignupBdayErrorMssg; EditText txtEmail;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.signup_page);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            Instance = this;
            Settings.ActivityInt = (int)ActivityType.SignUpActivity;

            btnSignup = FindViewById<Button>(Resource.Id.btnSignupIndividual);
            txtDisplayName = FindViewById<EditText>(Resource.Id.txtSignupDisplayName);
            txtEmail = FindViewById<EditText>(Resource.Id.txtSignupEmail);
            txtPassword = FindViewById<EditText>(Resource.Id.txtSignupPassword);
            txtConfirmPassword = FindViewById<EditText>(Resource.Id.txtSignupConfirmPassword);
            txtCountry = FindViewById<Spinner>(Resource.Id.txtSignupCountry);
            txtState = FindViewById<Spinner>(Resource.Id.txtSignupState);
            txtBday = FindViewById<TextView>(Resource.Id.txtSignupBday);
            txtSignupBdayErrorMssg = FindViewById<TextInputLayout>(Resource.Id.txtSignupBdayErrorMssg);
            var chkIagree = FindViewById<CheckBox>(Resource.Id.chkSignupTermsofService);
            txtTermsofService = FindViewById<TextView>(Resource.Id.lblSignupTermsofService);

            SpinAccounType.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(SpinAccountType_ItemSelected);
            SpinGroupType.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(SpinGroupType_ItemSelected);

            var ArrAccntType = new string[] { "Group", "Individual"};
            var AadapterAccntType = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, ArrAccntType);
            AadapterAccntType.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            SpinAccounType.Adapter = null;
            SpinAccounType.Adapter = AadapterAccntType;

            var ArrGroupType = new string[] { "Family", "Organization" };
            var AadapterGroupType = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, ArrGroupType);
            AadapterGroupType.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            SpinGroupType.Adapter = null;
            SpinGroupType.Adapter = AadapterGroupType;

            btnSignup.Enabled = false;
            btnSignup.SetBackgroundResource(Resource.Drawable.myButtonDisabled);
            txtBday.Click += DateSelect_OnClick;
            txtDisplayName.TextChanged += onEditTextChange;
            txtPassword.TextChanged += onEditTextChange;
            txtConfirmPassword.TextChanged += onEditTextChange;
            txtBday.TextChanged += onEditTextChange;

            loadCountries();

            txtEmail.TextChanged += onEditTextChange;
            //Signup Button is clicked
            btnSignup.Click += async (object sender, EventArgs e) =>
            {
                if (chkIagree.Checked)
                {
                    var email = txtEmail.Text;
                    var password = txtPassword.Text;
                    var displayname = txtDisplayName.Text;
                    var country = (string)txtCountry.SelectedItem;
                    var bday = DateTime.Parse(txtBday.Text);
                    string imgPhoto = ImgUserPhoto.ContentDescription;
                    if (!URLUtil.IsValidUrl(ImgUserPhoto.ContentDescription))
                    {
                        imgPhoto = "data:image/jpeg;base64," + ImgUserPhoto.ContentDescription;
                    }

                    this.Window.AddFlags(WindowManagerFlags.NotTouchable);
                    LinearProgress.Visibility = ViewStates.Visible;
                    var resultModel = await SharedService.AccountService.Instance.SignUpAsync(email,
                                        password, displayname, country, bday, imgPhoto, SpinAccounType.ContentDescription,SpinGroupType.ContentDescription, EditOwnerName.Text);
                    LinearProgress.Visibility = ViewStates.Gone;
                    this.Window.ClearFlags(WindowManagerFlags.NotTouchable);

                    var objBuilder = new AlertDialog.Builder(this);
                    objBuilder.SetTitle("");
                    objBuilder.SetMessage(resultModel.ResultMessage);
                    objBuilder.SetCancelable(false);

                    AlertDialog objDialog = objBuilder.Create();
                    objDialog.SetButton((int)(DialogButtonType.Positive), "OK", (s, ev) =>
                    {
                        if (resultModel.ResultEnum == Shared.Helpers.Result.Success)
                        {
                            this.Finish();
                        }
                    });
                    objDialog.Show();
                }
                else
                {
                    var objBuilder = new AlertDialog.Builder(this);
                    objBuilder.SetTitle("");
                    objBuilder.SetMessage("Please accept Terms and Conditions to continue.");
                    objBuilder.SetCancelable(false);

                    AlertDialog objDialog = objBuilder.Create();
                    objDialog.SetButton((int)(DialogButtonType.Positive), "OK", (s, ev) => { });
                    objDialog.Show();
                }
            };
            //Terms of Service is clicked
            txtTermsofService.Click += async (object sender, EventArgs e) =>
            {
                await Browser.OpenAsync("https://tokket.com/terms", new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Show,
                    PreferredToolbarColor = System.Drawing.Color.AliceBlue,
                    PreferredControlColor = System.Drawing.Color.Violet
                });
            };
        }

        private void onEditTextChange(object sender , Android.Text.TextChangedEventArgs e)
        {
            var returnvalue = true;
            if (string.IsNullOrEmpty(txtDisplayName.Text.Trim()))
            {
                txtDisplayName.Error = "Display name must have a value.";
                returnvalue = false;
            }

            if (txtPassword.Text.Trim().Length < 7)
            {
                txtPassword.Error = "Password must be greater than 6 characters.";
                returnvalue = false;
            }
            else
            {
                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    txtPassword.Error = "Passwords do not match.";
                    returnvalue = false;
                }
                else
                {
                    txtPassword.Error = null;
                }
            }

            if (!Android.Util.Patterns.EmailAddress.Matcher(txtEmail.Text).Matches())
            {
                txtEmail.Error = "Invalid Email Address";
                returnvalue = false;
            }

            var today = DateTime.Today;
            var age = 0;
            if (!string.IsNullOrEmpty(txtBday.Text))
            {
                age = today.Year - DateTime.Parse(txtBday.Text).Year;
            };
            //if (DateTime.Parse(txtBday.Text).Date > today.AddYears(-age)) age--;
            if (age < 13)
            {
                txtSignupBdayErrorMssg.Error = "You must be 13yrs old or above to register.";
                returnvalue = false;
            }
            else
            {
                txtSignupBdayErrorMssg.Error = string.Empty;
            }
            
            btnSignup.Enabled = returnvalue;
            if (btnSignup.Enabled == true)
            {
                btnSignup.SetBackgroundResource(Resource.Drawable.myButton);
            }
            else
            {
                btnSignup.SetBackgroundResource(Resource.Drawable.myButtonDisabled);
            }
        }
        public void loadCountries()
        {
            txtCountry.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(txtCountry_ItemSelected);
            List<CountryModel> countryModels = CountryHelper.GetCountries();
            List<string> countriesList = new List<string>();
            for (int i = 0; i < countryModels.Count(); i++)
            {
                countriesList.Add(countryModels[i].Name);
            }
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, countriesList);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            txtCountry.Adapter = adapter;
        }
        private void txtCountry_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            if (spinner.GetItemAtPosition(e.Position).ToString().ToLower() == "united states")
            {
                //linearState.Visibility = ViewStates.Visible;
                LinearState.Visibility = ViewStates.Visible;
                loadState("us");
            }
            else
            {
                LinearState.Visibility = ViewStates.Gone;
                txtState.Adapter = null;
            }
        }
        public void loadState(string countryId)
        {
            List<Shared.Models.StateModel> stateModel = CountryHelper.GetCountryStates(countryId);
            List<string> statelist = new List<string>();
            for (int i = 0; i < stateModel.Count(); i++)
            {
                statelist.Add(stateModel[i].Name);
            }
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, statelist);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            txtState.Adapter = adapter;
        }
        void DateSelect_OnClick(object sender, EventArgs eventArgs)
        {
            datepicker_fragment frag = datepicker_fragment.NewInstance(delegate (DateTime time)
            {
                txtBday.Text = time.ToString("dd/MM/yyyy");
            });
            frag.Show(SupportFragmentManager, datepicker_fragment.TAG);
        }
        private void SpinAccountType_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            SpinAccounType.ContentDescription = SpinAccounType.GetItemAtPosition(SpinAccounType.FirstVisiblePosition).ToString();
            switch (SpinAccounType.ContentDescription.ToLower())
            {
              case "group":
                    btnSignup.Text = "Register Group ($2.99 USD)";
                    LinearGroupType.Visibility = ViewStates.Visible;
                    LinearGroupName.Visibility = ViewStates.Visible;
                    TextDisplayNameHeader.Text = "Family's Last Name or Display Name ('Family' will be appended)";
                    break;
              case "individual":
                    EditOwnerName.Text = "";
                    btnSignup.Text = "Register";
                    LinearGroupType.Visibility = ViewStates.Gone;
                    LinearGroupName.Visibility = ViewStates.Gone;
                    TextDisplayNameHeader.Text = "Display Name";
                    break;
            }
        }
        [Java.Interop.Export("OnClickAddTokImgMain")]
        public void OnClickAddTokImgMain(View v)
        {
            Settings.BrowsedImgTag = -1;
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), (int)ActivityType.SignUpActivity);
        }
        [Java.Interop.Export("OnClickRemoveTokImgMain")]
        public void OnClickRemoveTokImgMain(View v)
        {
            ImgUserPhoto.SetImageBitmap(null);

            BtnBrowseImg.Visibility = ViewStates.Visible;
            BtnRemoveImg.Visibility = ViewStates.Gone;
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == (int)ActivityType.SignUpActivity) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
                Intent nextActivity = new Intent(this, typeof(CropImageActivity));
                Settings.ImageBrowseCrop = (string)uri;
                this.StartActivityForResult(nextActivity, requestCode);
            }

        }
        public void displayImageBrowse()
        {
            //Main Image
            ImgUserPhoto.SetImageBitmap(null);
            if (Settings.BrowsedImgTag == -1)
            {
                //AddTokVm.TokModel.Image = Settings.ImageBrowseCrop;
                ImgUserPhoto.ContentDescription = Settings.ImageBrowseCrop;
                byte[] imageMainBytes = Convert.FromBase64String(Settings.ImageBrowseCrop);
                ImgUserPhoto.SetImageBitmap((BitmapFactory.DecodeByteArray(imageMainBytes, 0, imageMainBytes.Length)));
                BtnBrowseImg.Visibility = ViewStates.Gone;
                BtnRemoveImg.Visibility = ViewStates.Visible;
            }
            Settings.ImageBrowseCrop = null;
        }
        public void SpinGroupType_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            SpinGroupType.ContentDescription = SpinGroupType.GetItemAtPosition(SpinGroupType.FirstVisiblePosition).ToString();
            switch (SpinGroupType.ContentDescription.ToLower())
            {
                case "family":
                    TextGroupTypeHeader.Text = "Account Owner's First Name";
                    TextDisplayNameHeader.Text = "Family's Last Name or Display Name ('Family' will be appended)";
                    break;
                case "organization":
                    TextGroupTypeHeader.Text = "Account Owner's Name";
                    TextDisplayNameHeader.Text = "Organization Name";
                    break;
            }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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
        public Spinner SpinAccounType => FindViewById<Spinner>(Resource.Id.SpinnerSignupAccountType);
        public Spinner SpinGroupType => FindViewById<Spinner>(Resource.Id.SpinnerSignupGroupType);
        public TextView TextGroupTypeHeader => FindViewById<TextView>(Resource.Id.TextGroupTypeHeader);
        public EditText EditOwnerName => FindViewById<EditText>(Resource.Id.EditSignupOwnerName);
        public LinearLayout LinearGroupType => FindViewById<LinearLayout>(Resource.Id.LinearSignUpGroupType); 
        public LinearLayout LinearGroupName => FindViewById<LinearLayout>(Resource.Id.LinearSignUpGroupOwnerName);
        public TextView TextDisplayNameHeader => FindViewById<TextView>(Resource.Id.TextHeaderDisplayName); 
        public LinearLayout LinearProgress => FindViewById<LinearLayout>(Resource.Id.LinearProgress_Signup);
        public LinearLayout LinearState => FindViewById<LinearLayout>(Resource.Id.linearState);
        public ImageView ImgUserPhoto => FindViewById<ImageView>(Resource.Id.ImgSignupPhoto);
        public Button BtnBrowseImg => FindViewById<Button>(Resource.Id.btnSignupBrowseImg);
        public Button BtnRemoveImg => FindViewById<Button>(Resource.Id.btnSignupRemoveImg);
    }
}