using Foundation;
using System;
using Tokkepedia.iOS.Views;
using UIKit;
using Xamarin.Forms;

namespace Tokkepedia.iOS
{
    public partial class LeftViewController : UIViewController
    {
        public SettingsNavigationController _navigation;
        public SetsNavigationController _setsNavigation;

        IntPtr ptr;
        public LeftViewController (IntPtr handle) : base (handle)
        {
            ptr = handle;
        }

        partial void BtnPrivacy_TouchUpInside(UIButton sender)
        {
            OpenAddEditSet();
        }

        partial void BtnTheme_TouchUpInside(UIButton sender)
        {
            OpenAddEditGroup();
        }

        partial void BtnSettings_TouchUpInside(UIButton sender)
        {
            var settingsPage = new SettingsPage()
            {
                Title = "Settings",
                EnableBackButtonOverride = false
                //BindingContext = note
            };
            _navigation = new SettingsNavigationController() { NavigationControl = this.NavigationController };
            settingsPage.NavigationController = _navigation;

            var iOSSettingsPage = (settingsPage).CreateViewController();

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
            UIApplication.SharedApplication.KeyWindow.RootViewController.HidesBottomBarWhenPushed = true; //= _navigation;
            
            this.NavigationController?.PushViewController(iOSSettingsPage, true);
        }

        partial void BtnSet_TouchUpInside(UIButton sender)
        {
            //var page = new SetsPage()
            //{
            //    Title = "Sets"
            //};
            //_setsNavigation = new SetsNavigationController() { NavigationControl = this.NavigationController };
            //page.NavigationController = _setsNavigation;

            //var iOSPage = (page).CreateViewController();

            //UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
            //UIApplication.SharedApplication.KeyWindow.RootViewController.HidesBottomBarWhenPushed = true; //= _navigation;

            //NavigationController.HidesBottomBarWhenPushed = true;
            //this.NavigationController?.PushViewController(iOSPage, true);



            var page = new SetsPage()
            {
                Title = "Sets"
            };
            _setsNavigation = new SetsNavigationController() { NavigationControl = this.NavigationController };

            var iOSPage = (page).CreateViewController();

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();

            var tab = UIApplication.SharedApplication.KeyWindow.RootViewController as UITabBarController;
            var nav = tab.SelectedViewController as UINavigationController;

            nav.NavigationBar.TintColor = UIColor.White;
            nav.HidesBottomBarWhenPushed = true;


            //Edges: https://stackoverflow.com/a/19143661
            iOSPage.EdgesForExtendedLayout = UIRectEdge.None;
            
            iOSPage.ToolbarItems = new UIBarButtonItem[]
            {
                new UIBarButtonItem("Tok Cards", UIBarButtonItemStyle.Bordered, null),
                new UIBarButtonItem("Tok Match", UIBarButtonItemStyle.Bordered, null),
                new UIBarButtonItem("Tok Choice", UIBarButtonItemStyle.Bordered, null)
            };

            this.DismissViewController(true, OnClicked);
            nav?.PushViewController(iOSPage, true);

            //nav.NavigationBar.TopItem.Title = "Tok Choice";
            nav.NavigationBar.TopItem.Title = "Sets";

            //// Create a new Alert Controller
            //UIAlertController actionSheetAlert = UIAlertController.Create("Play", "Select a game:", UIAlertControllerStyle.ActionSheet);

            //// Add Actions
            //actionSheetAlert.AddAction(UIAlertAction.Create("Tok Cards", UIAlertActionStyle.Default, (action) => OpenTokCards()));

            //actionSheetAlert.AddAction(UIAlertAction.Create("Tok Match", UIAlertActionStyle.Default, (action) => OpenTokMatch()));

            //actionSheetAlert.AddAction(UIAlertAction.Create("Tok Choice", UIAlertActionStyle.Default, (action) => OpenTokChoice()));

            //actionSheetAlert.AddAction(UIAlertAction.Create("Set", UIAlertActionStyle.Default, (action) => OpenAddEditSet()));

            //actionSheetAlert.AddAction(UIAlertAction.Create("Group", UIAlertActionStyle.Default, (action) => OpenAddEditGroup()));

            //actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Destructive, null));

            //// Required for iPad - You must specify a source for the Action Sheet since it is
            //// displayed as a popover
            //UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
            //if (presentationPopover != null)
            //{
            //    presentationPopover.SourceView = this.View;
            //    presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            //}

            //// Display the alert
            //this.PresentViewController(actionSheetAlert, true, null);
        }

        void OpenTokCards()
        {
            var page = new TokCardsPage()
            {
                Title = "Tok Cards"
            };
            _navigation = new SettingsNavigationController() { NavigationControl = this.NavigationController };
            //page.NavigationController = _navigation;

            var iOSPage = (page).CreateViewController();

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
            UIApplication.SharedApplication.KeyWindow.RootViewController.HidesBottomBarWhenPushed = true; //= _navigation;

            NavigationController.HidesBottomBarWhenPushed = true;
            this.NavigationController?.PushViewController(iOSPage, true);
        }

        void OpenTokMatch()
        {
            var page = new TokMatchPage()
            {
                Title = "Tok Match"
            };
            _navigation = new SettingsNavigationController() { NavigationControl = this.NavigationController };
            page.NavigationController = _navigation;

            var iOSMatchPage = (page).CreateViewController();

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
            UIApplication.SharedApplication.KeyWindow.RootViewController.HidesBottomBarWhenPushed = true; //= _navigation;

            this.NavigationController?.PresentViewController(iOSMatchPage, true, OnClicked);
        }

        void OpenTokChoice()
        {
            var page = new TokBackPage()
            {
                Title = "Tok Back"
            };
            _navigation = new SettingsNavigationController() { NavigationControl = this.NavigationController };

            var iOSPage = (page).CreateViewController();

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();

            var tab = UIApplication.SharedApplication.KeyWindow.RootViewController as UITabBarController;
            var nav = tab.SelectedViewController as UINavigationController;
            
            nav.NavigationBar.TintColor = UIColor.White;
            nav.HidesBottomBarWhenPushed = true;
            

            //Edges: https://stackoverflow.com/a/19143661
            iOSPage.EdgesForExtendedLayout = UIRectEdge.None;

            this.DismissViewController(true, OnClicked);
            nav?.PushViewController(iOSPage, true);

            //nav.NavigationBar.TopItem.Title = "Tok Choice";
            nav.NavigationBar.TopItem.Title = "Home";
        }

        void OpenAddEditSet()
        {
            var page = new AddEditSetPage()
            {
                Title = "Add Class Set"
            };
            _navigation = new SettingsNavigationController() { NavigationControl = this.NavigationController };

            var iOSPage = (page).CreateViewController();

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();

            var tab = UIApplication.SharedApplication.KeyWindow.RootViewController as UITabBarController;
            var nav = tab.SelectedViewController as UINavigationController;

            nav.NavigationBar.TintColor = UIColor.White;
            nav.HidesBottomBarWhenPushed = true;


            //Edges: https://stackoverflow.com/a/19143661
            iOSPage.EdgesForExtendedLayout = UIRectEdge.None;

            this.DismissViewController(true, OnClicked);
            nav?.PushViewController(iOSPage, true);

            //nav.NavigationBar.TopItem.Title = "Tok Choice";
            nav.NavigationBar.TopItem.Title = "Home";
        }

        void OpenAddEditGroup()
        {
            var page = new AddEditGroupPage()
            {
                Title = "Add Class Group"
            };

            var iOSPage = (page).CreateViewController();

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();

            var tab = UIApplication.SharedApplication.KeyWindow.RootViewController as UITabBarController;
            var nav = tab.SelectedViewController as UINavigationController;

            nav.NavigationBar.TintColor = UIColor.White;
            nav.HidesBottomBarWhenPushed = true;


            //Edges: https://stackoverflow.com/a/19143661
            iOSPage.EdgesForExtendedLayout = UIRectEdge.None;

            this.DismissViewController(true, OnClicked);
            nav?.PushViewController(iOSPage, true);

            //nav.NavigationBar.TopItem.Title = "Tok Choice";
            nav.NavigationBar.TopItem.Title = "Home";
        }

        void OpenOther()
        {
            var tok = UIStoryboard.FromName("TokInfoViewController", null).InstantiateViewController("tokInfo") as TokInfoController;

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
            UIApplication.SharedApplication.KeyWindow.RootViewController.HidesBottomBarWhenPushed = false; //= _navigation;

            this.NavigationController?.PushViewController(tok, true);
        }

        void OnClicked() { }
    }
}