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
    [Register ("SearchController")]
    partial class SearchController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UINavigationItem NavItem { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView searchViewContainer { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (NavItem != null) {
                NavItem.Dispose ();
                NavItem = null;
            }

            if (searchViewContainer != null) {
                searchViewContainer.Dispose ();
                searchViewContainer = null;
            }
        }
    }
}