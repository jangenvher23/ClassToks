using Foundation;
using System;
using UIKit;

namespace Tokkepedia.iOS
{
    public partial class ContainerController : UIViewController
    {
        UIViewController menuController;
        UITabBarController tabController;
        UINavigationController navController;

        bool isExpanded = false;

        UIStoryboard myStoryboard = UIStoryboard.FromName("Main", null);

        public ContainerController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            tabController = myStoryboard.InstantiateViewController("MainTabController") as UITabBarController;

          
            View.InsertSubview(tabController.View, 0);
            AddChildViewController(tabController);
            tabController.DidMoveToParentViewController(this);

            btnMenu.TouchUpInside += ButtonClickedAction;
            configureMenuController();
        }


        
        public void configureMenuController()
        {
            if(menuController == null)
            {
                menuController = myStoryboard.InstantiateViewController("SideMenuController") as UIViewController;
                View.InsertSubview(menuController.View, 0);
                AddChildViewController(menuController);
                menuController.DidMoveToParentViewController(this);
            }
        }

        public void configureNavController()
        {
            
        }

        public void showMenuController(bool shouldExpand)
        {
            if (shouldExpand)
            {
                UIView.Animate(0.3, 0, UIViewAnimationOptions.CurveEaseInOut, () => {
                    tabController.View.Frame = new CoreGraphics.CGRect(new CoreGraphics.CGPoint(tabController.View.Frame.Width - 80, tabController.View.Frame.Y), tabController.View.Frame.Size);
                }, null);
            }
            else
            {
                UIView.Animate(0.3, 0, UIViewAnimationOptions.CurveEaseInOut, () => {
                    tabController.View.Frame = new CoreGraphics.CGRect(new CoreGraphics.CGPoint(0, tabController.View.Frame.Y), tabController.View.Frame.Size);
                }, null);
            }
        }

        private void ButtonClickedAction(object sender, EventArgs e)
        {
            isExpanded = !isExpanded;
            showMenuController(isExpanded);
        }
    }
}                    