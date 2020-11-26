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
    [Register ("PickerViewController")]
    partial class PickerViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSelect { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIPickerView txtPicker { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnSelect != null) {
                btnSelect.Dispose ();
                btnSelect = null;
            }

            if (txtPicker != null) {
                txtPicker.Dispose ();
                txtPicker = null;
            }
        }
    }
}