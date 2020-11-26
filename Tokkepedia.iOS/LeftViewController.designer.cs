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
    [Register ("LeftViewController")]
    partial class LeftViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSet { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSettings { get; set; }

        [Action ("BtnPrivacy_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void BtnPrivacy_TouchUpInside (UIKit.UIButton sender);

        [Action ("BtnSet_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void BtnSet_TouchUpInside (UIKit.UIButton sender);

        [Action ("BtnSettings_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void BtnSettings_TouchUpInside (UIKit.UIButton sender);

        [Action ("BtnTheme_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void BtnTheme_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnSet != null) {
                btnSet.Dispose ();
                btnSet = null;
            }

            if (btnSettings != null) {
                btnSettings.Dispose ();
                btnSettings = null;
            }
        }
    }
}