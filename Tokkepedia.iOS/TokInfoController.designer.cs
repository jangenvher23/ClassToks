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
    [Register ("TokInfoController")]
    partial class TokInfoController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView lblTitleSubaccount { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView lblUserDisplayName { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblTitleSubaccount != null) {
                lblTitleSubaccount.Dispose ();
                lblTitleSubaccount = null;
            }

            if (lblUserDisplayName != null) {
                lblUserDisplayName.Dispose ();
                lblUserDisplayName = null;
            }
        }
    }
}