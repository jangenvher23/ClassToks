using CoreGraphics;
using FloatingActionButton;
using Foundation;
using System;
using System.Diagnostics;
using Tokkepedia.iOS.ViewModels;
using Tokkepedia.iOS.Views;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Xamarin.SideMenu;
using static Tokkepedia.iOS.Views.WindowManager;

namespace Tokkepedia.iOS
{
    public partial class HomeController : UIViewController
    {
        public HomeController (IntPtr handle) : base (handle)
        {
        }

        public MainPageViewModel MainVM => App.Locator.MainPageVM;

        //private UICollectionView collectionView;
        public UICollectionView CollectionView
        {
            get
            {
                mainCollectionView.DraggingStarted += CollectionView_Scrolled;
                return mainCollectionView;
            }
        }

        private SideMenuManager sideMenuManager;
        private UIViewController leftSideController;
        private UIViewController rightSideController;

        private UISideMenuNavigationController left;

        private UIRefreshControl refreshControl;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CollectionView.RegisterClassForCell(typeof(TokTileViewCell), TokTileViewCell.CellID);
            CollectionView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Always;

            #region Refresh
            // Create the UIRefreshControl
            refreshControl = new UIRefreshControl();

            // Handle the pullDownToRefresh event
            refreshControl.ValueChanged += RefreshFeed;

            // Add the UIRefreshControl to the TableView
            CollectionView.AddSubview(refreshControl);
            #endregion

            MainVM.CollectionView = CollectionView;
            

            //Data
            InvokeOnMainThread(async () => await MainVM.InitAsync());

            this.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(UIImage.FromBundle("hamburger-menu.png"), UIBarButtonItemStyle.Plain, (sender, e) =>
            {
                PresentViewController(sideMenuManager.LeftNavigationController, true, null);
            }),
               false);

            this.NavigationItem.SetRightBarButtonItem(
                new UIBarButtonItem("Filter", UIBarButtonItemStyle.Bordered, (sender, e) =>
                {
                    PresentViewController(sideMenuManager.RightNavigationController, true, null);
                }),
                false);
            sideMenuManager = new SideMenuManager();
            this.NavigationItem.LeftBarButtonItem.TintColor = UIColor.White;
            this.NavigationItem.RightBarButtonItem.TintColor = UIColor.White;

            SetupSideMenu();
            SetDefaults();

            FloatButton = new FloatingButton(70) { ButtonColor = UIColor.SystemPurpleColor, PlusColor = UIColor.White, HasShadow = true, AnimationSpeed = 0.3, PaddingY = 100 };
            FloatButton.Center = UIApplication.SharedApplication.KeyWindow.Center;
            FloatButton.EmptyFloatySelected += (sender, e) => {
                OpenAddEditTok();
            };
            UIApplication.SharedApplication.KeyWindow.AddSubview(FloatButton);
        }

        private void CollectionView_Scrolled(object sender, EventArgs e)
        {
            Debug.WriteLine($"{sender}: {e}");

        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (WindowManager.FloatButton != null)
                WindowManager.FloatButton.Hidden = false;
            else
            {
                //FloatButton = new FloatingButton(40) { ButtonColor = UIColor.SystemPurpleColor, PlusColor = UIColor.White, 
                //    PaddingX = 120, PaddingY = 120,  Hidden = false,
                //    HasShadow = true, AnimationSpeed = 0.3 };
                //FloatButton.Center = UIApplication.SharedApplication.KeyWindow.Center;
                //FloatButton.EmptyFloatySelected += (sender, e) => { //Tap button
                //    OpenAddEditTok();
                //};
                //UIApplication.SharedApplication.KeyWindow.AddSubview(FloatButton);
            }
        }

        private async void RefreshFeed(object sender, EventArgs e)
        {
            await MainVM.InitAsync();
            refreshControl.EndRefreshing();
            MainVM.CollectionView.ReloadData();
        }

        void OpenAddEditTok()
        {
            var page = new AddEditTokPage()
            {
                Title = "Add Tok"
            };

            var iOSPage = (page).CreateViewController();

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();

            var tab = UIApplication.SharedApplication.KeyWindow.RootViewController as UITabBarController;
            var nav = tab.SelectedViewController as UINavigationController;

            nav.NavigationBar.TintColor = UIColor.White;
            nav.HidesBottomBarWhenPushed = true;


            //Edges: https://stackoverflow.com/a/19143661
            iOSPage.EdgesForExtendedLayout = UIRectEdge.None;


            nav?.PresentViewController(iOSPage, true, null);

            //nav.NavigationBar.TopItem.Title = "Tok Choice";
            nav.NavigationBar.TopItem.Title = "Home";
        }

        void SetupSideMenu()
        {
            GetViewControllers();

            left = new UISideMenuNavigationController(sideMenuManager, leftSideController, leftSide: true);
            left.NavigationBarHidden = false;
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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            //hide
            //NavigationController.NavigationBarHidden = true;
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            CollectionView.CollectionViewLayout.InvalidateLayout();

         
        }


        private void AnimationAction()
        {
            this.View.LayoutIfNeeded();
        }

        private void AnimationCompletionHandler()
        {
            //Completion of Animation
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}