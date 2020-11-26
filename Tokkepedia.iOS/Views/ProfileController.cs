using Foundation;
using System;
using UIKit;
using Xamarin.SideMenu;

namespace Tokkepedia.iOS
{
    public partial class ProfileController : UIViewController
    {
        public ProfileController (IntPtr handle) : base (handle)
        {
        }

        private SideMenuManager sideMenuManager;
        private UIViewController leftSideController;
        private UIViewController rightSideController;


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(UIImage.FromBundle("hamburger-menu.png"), UIBarButtonItemStyle.Plain, (sender, e) =>
            {
                PresentViewController(sideMenuManager.LeftNavigationController, true, null);
            }),
               false);

            this.NavigationItem.SetRightBarButtonItem(
                new UIBarButtonItem("Filter", UIBarButtonItemStyle.Plain, (sender, e) =>
                {
                    PresentViewController(sideMenuManager.RightNavigationController, true, null);
                }),
                false);
            sideMenuManager = new SideMenuManager();
            this.NavigationItem.LeftBarButtonItem.TintColor = UIColor.White;
            this.NavigationItem.RightBarButtonItem.TintColor = UIColor.White;

            SetupSideMenu();
            SetDefaults();
        }
        void SetupSideMenu()
        {
            GetViewControllers();

            var left = new UISideMenuNavigationController(sideMenuManager, leftSideController, leftSide: true);
            left.NavigationBarHidden = true;
            sideMenuManager.LeftNavigationController = left;
            sideMenuManager.RightNavigationController = new UISideMenuNavigationController(sideMenuManager, rightSideController, leftSide: false);

            // Enable gestures. The left and/or right menus must be set up above for these to work.
            // Note that these continue to work on the Navigation Controller independent of the View Controller it displays!
            sideMenuManager.AddPanGestureToPresent(toView: this.NavigationController?.NavigationBar);

            sideMenuManager.AddScreenEdgePanGesturesToPresent(toView: this.NavigationController?.View);

        }

        private void GetViewControllers()
        {
            leftSideController = Storyboard.InstantiateViewController("LeftViewController") as LeftViewController;
            rightSideController = Storyboard.InstantiateViewController("RightViewController") as RightViewController;
        }


        void SetDefaults()
        {
            //sideMenuManager.BlurEffectStyle = null;
            //sideMenuManager.AnimationFadeStrength = 5d;
            //sideMenuManager.ShadowOpacity = 5d;
            //sideMenuManager.AnimationTransformScaleFactor = 5d;
            //sideMenuManager.FadeStatusBar = true;
        }
    }
}