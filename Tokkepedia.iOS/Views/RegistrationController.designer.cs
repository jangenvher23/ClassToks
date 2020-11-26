// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Tokkepedia.iOS
{
    [Register ("RegistrationController")]
    partial class RegistrationController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnRegister { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnState { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnTerms { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblBirthdayError { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblConfirmPasswordError { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblDisplayNameError { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblEmailError { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblPasswordError { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UINavigationBar SignUp { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIStackView stackBirthday { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIStackView stackCountry { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIStackView stackState { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch switchTerms { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIDatePicker txtBirthday { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtConfirmPassword { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIPickerView txtCountry { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtDisplayName { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtEmail { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtPassword { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnRegister != null) {
                btnRegister.Dispose ();
                btnRegister = null;
            }

            if (btnState != null) {
                btnState.Dispose ();
                btnState = null;
            }

            if (btnTerms != null) {
                btnTerms.Dispose ();
                btnTerms = null;
            }

            if (lblBirthdayError != null) {
                lblBirthdayError.Dispose ();
                lblBirthdayError = null;
            }

            if (lblConfirmPasswordError != null) {
                lblConfirmPasswordError.Dispose ();
                lblConfirmPasswordError = null;
            }

            if (lblDisplayNameError != null) {
                lblDisplayNameError.Dispose ();
                lblDisplayNameError = null;
            }

            if (lblEmailError != null) {
                lblEmailError.Dispose ();
                lblEmailError = null;
            }

            if (lblPasswordError != null) {
                lblPasswordError.Dispose ();
                lblPasswordError = null;
            }

            if (SignUp != null) {
                SignUp.Dispose ();
                SignUp = null;
            }

            if (stackBirthday != null) {
                stackBirthday.Dispose ();
                stackBirthday = null;
            }

            if (stackCountry != null) {
                stackCountry.Dispose ();
                stackCountry = null;
            }

            if (stackState != null) {
                stackState.Dispose ();
                stackState = null;
            }

            if (switchTerms != null) {
                switchTerms.Dispose ();
                switchTerms = null;
            }

            if (txtBirthday != null) {
                txtBirthday.Dispose ();
                txtBirthday = null;
            }

            if (txtConfirmPassword != null) {
                txtConfirmPassword.Dispose ();
                txtConfirmPassword = null;
            }

            if (txtCountry != null) {
                txtCountry.Dispose ();
                txtCountry = null;
            }

            if (txtDisplayName != null) {
                txtDisplayName.Dispose ();
                txtDisplayName = null;
            }

            if (txtEmail != null) {
                txtEmail.Dispose ();
                txtEmail = null;
            }

            if (txtPassword != null) {
                txtPassword.Dispose ();
                txtPassword = null;
            }
        }
    }
}