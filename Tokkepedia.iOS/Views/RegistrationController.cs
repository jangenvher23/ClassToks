using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using Tokkepedia.iOS.ViewModels;
using Tokkepedia.Shared.Helpers;
using Tokket.Tokkepedia.Tools;                             
using SharedService = Tokkepedia.Shared.Services;
using UIKit;
using Xamarin.Essentials;
using System.Drawing;
using System.Text.RegularExpressions;
using CoreGraphics;

namespace Tokkepedia.iOS
{
    public partial class RegistrationController : UITableViewController
    {
        PickerViewModel countryPickerVM;
        List<string> statelist = new List<string>();


        public string Birthday { get; set; }
        public string Country { get; set; }
        public string State { get; set; }

        public RegistrationController (IntPtr handle) : base (handle)
        {
           
        }

        //public override UIModalPresentationStyle ModalPresentationStyle {
        //    get => UIModalPresentationStyle.FullScreen; 
        //    set => base.ModalPresentationStyle = value;
        //}

        UIStoryboard myStoryboard = UIStoryboard.FromName("Main", null);
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            loadCountries();

            btnState.Layer.BorderWidth = 1;
            btnState.Layer.CornerRadius = 4;
            btnState.Layer.BorderColor = UIColor.LightGray.CGColor;


            //Signup Button is clicked
            btnRegister.TouchDown += async (object sender, EventArgs e) =>
            {
                if (switchTerms.On)
                {
                    var email = txtEmail.Text;
                    var password = txtPassword.Text;
                    var displayname = txtDisplayName.Text;
                    var country = countryPickerVM.SelectedItem;
                    var bday = DateTime.Parse(txtBirthday.Date.ToString());
                    //TODO: Include state and group acccount logic
                    var resultModel = await SharedService.AccountService.Instance.SignUpAsync(email,
                                        password, displayname, country, bday, "", "individual", null, null);
                    if (resultModel.ResultEnum == Shared.Helpers.Result.Success)
                    {
                        this.NavigationController.PopViewController(true);
                    }
                    else
                    {                                             
                        var okAlertController = UIAlertController.Create("", resultModel.ResultMessage, UIAlertControllerStyle.Alert) ;
                        okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        PresentViewController(okAlertController, true, null);
                    }
                }
                else
                {
                    var okAlertController = UIAlertController.Create("", "Please accept Terms and Conditions to continue.", UIAlertControllerStyle.Alert);
                    okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(okAlertController, true, null);
                }
            };


           

            btnState.TouchUpInside += async (object sender, EventArgs e) =>
            {
                PickerViewController vc2 = Storyboard.InstantiateViewController("PickerViewController") as PickerViewController;
                vc2.ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;
                vc2.pickerViewModel = new PickerViewModel(statelist);
                PresentViewController(vc2, true, null);                                 
            };

            //Terms of Service is clicked
            btnTerms.TouchDown += async (object sender, EventArgs e) =>
            {
                await Browser.OpenAsync("https://tokket.com/terms", new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Show,
                    PreferredToolbarColor = Color.AliceBlue,
                    PreferredControlColor = Color.Violet
                });
            };

            //Value Change events                 
            txtDisplayName.AddTarget(onEditTextChange, UIControlEvent.EditingChanged);
            txtBirthday.AddTarget(onEditTextChange, UIControlEvent.EditingChanged);
            txtEmail.AddTarget(onEditTextChange, UIControlEvent.EditingChanged);
            txtPassword.AddTarget(onEditTextChange, UIControlEvent.EditingChanged);
            txtConfirmPassword.AddTarget(onEditTextChange, UIControlEvent.EditingChanged);

            //hides keypad when not focused on textfield
            var g = new UITapGestureRecognizer(() => View.EndEditing(true));
            g.CancelsTouchesInView = false; //for iOS5 
            View.AddGestureRecognizer(g);

            UIToolbar toolBar = new UIToolbar(new CGRect(0, 0, 320, 44));
            UIBarButtonItem flexibleSpaceLeft = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null, null);
            UIBarButtonItem doneButton = new UIBarButtonItem("OK", UIBarButtonItemStyle.Done, this, new ObjCRuntime.Selector("DoneAction"));
            UIBarButtonItem[] list = new UIBarButtonItem[] { flexibleSpaceLeft, doneButton };
            toolBar.SetItems(list, false);

            txtCountry.ShowSelectionIndicator = true;
            txtEmail.InputAccessoryView = toolBar;
            txtEmail.InputView = txtCountry;
        }

        public void loadCountries()
        {
            List<CountryModel> countryModels = CountryHelper.GetCountries();
            List<string> countriesList = new List<string>();
            for (int i = 0; i < countryModels.Count(); i++)
            {
                countriesList.Add(countryModels[i].Name);
            }
            countryPickerVM = new PickerViewModel(countriesList);
            countryPickerVM.ValueChanged += txtCountry_SelectedIndexChanged;
            txtCountry.Model = countryPickerVM;
            
        }

        public void loadState(string countryId)
        {
            List<Shared.Models.StateModel> stateModel = CountryHelper.GetCountryStates(countryId);
            for (int i = 0; i < stateModel.Count(); i++)
            {
                statelist.Add(stateModel[i].Name);
            }
            
        }
        private void txtCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(countryPickerVM.SelectedItem.ToString().ToLower() == "united states")
            {
                //txtState.IsAccessibilityElement = true;
                loadState("us");
            }
            else
            {
               // txtState.IsAccessibilityElement = false;
               statelist = new List<string>();
            }
        }

        private void onEditTextChange(object sender, EventArgs e)
        {
            var returnvalue = true;
            if (string.IsNullOrEmpty(txtDisplayName.Text.Trim()))
            {
                lblDisplayNameError.Text = "*Display name must have a value.";
                returnvalue = false;
            }
            else
            {
                lblDisplayNameError.Text = null;
            }

            if (txtPassword.Text.Trim().Length < 7)
            {
                lblPasswordError.Text = "*Password must be greater than 6 characters.";
                returnvalue = false;
            }
            else
            {
                lblPasswordError.Text = null;
                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    lblConfirmPasswordError.Text = "*Passwords do not match.";
                    returnvalue = false;
                }
                else
                {
                    lblConfirmPasswordError.Text = null;                     
                }
            }

            if (Regex.Match(txtEmail.Text, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Success)
            {
                lblEmailError.Text = "Invalid Email Address";
                returnvalue = false;
            }
            else
            {
                lblEmailError.Text = null;
            }

            var today = DateTime.Today;
            var age = today.Year - DateTime.Parse(txtBirthday.Date.ToString()).Year;
            if (age < 13)
            {
                lblBirthdayError.Text = "You must be 13yrs old or above to register.";
                returnvalue = false;
            }
            else
            {
                lblBirthdayError.Text = null;
            }

            btnRegister.Enabled = returnvalue;
            if (!btnRegister.Enabled)
            {
                btnRegister.BackgroundColor = UIColor.FromRGBA(135, 75, 222, 255);
            }
            else
            {
                btnRegister.BackgroundColor = UIColor.Gray;
            }
        }

    }
}