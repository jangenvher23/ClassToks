using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Tokkepedia.iOS.Views
{
    public class SettingsNavigationController : UINavigationController
    {
        public UINavigationController NavigationControl;

        public SettingsNavigationController()//UIViewController page
        {

        }

        public override UIViewController PopViewController(bool animated)
        {
            //UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);
            //MainTabController mainController = Storyboard.InstantiateViewController("MainTabController") as MainTabController;
            //UIApplication.SharedApplication.Windows[0].RootViewController.TransitionAsync(UIApplication.SharedApplication.Windows[0].RootViewController, mainController, 0.7,
            //    UIViewAnimationOptions.TransitionCurlDown, OnPagePopped);//OnTransitionFinished

            return base.PopViewController(animated);

            //UIViewController topView = NavigationControl;
            //if (topView != null)
            //{
            //    // Dispose of ViewController on back navigation.
            //    topView.Dispose();
            //}
            //return base.PopViewController(animated);
        }

        //public new UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);
        public void PopPage()
        {
            //var res = PopViewController(true);

            UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);
            MainTabController mainController = Storyboard.InstantiateViewController("MainTabController") as MainTabController;
            UIApplication.SharedApplication.Windows[0].RootViewController = mainController;

            //UIApplication.SharedApplication.Windows[0].RootViewController.Transition(UIApplication.SharedApplication.Windows[0].RootViewController, mainController, 0.7, UIViewAnimationOptions.TransitionCurlDown, OnPagePopped, OnTransitionFinished);//OnTransitionFinished

            //var mainController = this.Storyboard.InstantiateViewController("MainTabController") as MainTabController;
            //UIApplication.SharedApplication.Windows[0].RootViewController = mainController;

            //this.DismissViewController(true, OnPagePopped);
            //NavigationControl.PopToRootViewController(true);
        }

        public void OnPagePopped()
        {

        }

        private void OnTransitionFinished(bool finished)
        {

        }

        public void LogOutUser()
        {
            SecureStorage.RemoveAll();

            UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);
            LoginController mainController = Storyboard.InstantiateViewController("LoginController") as LoginController;

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
            UIApplication.SharedApplication.KeyWindow.RootViewController = mainController; //= _navigation;

            //UIApplication.SharedApplication.Windows[0].RootViewController.Transition(UIApplication.SharedApplication.Windows[0].RootViewController, mainController, 0.7, UIViewAnimationOptions.TransitionCurlDown, OnPagePopped, OnTransitionFinished);
        }
    }
}