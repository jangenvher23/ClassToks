using CoreGraphics;
using Foundation;
using System;
using UIKit;

namespace Tokkepedia.iOS
{
    public partial class MainTabController : UITabBarController
    {
        

        public MainTabController (IntPtr handle) : base (handle)
        {

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            //hide
            //NavigationController.NavigationBarHidden = true;
        }

       
    }
}