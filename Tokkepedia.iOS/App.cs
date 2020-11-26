using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Tokkepedia.iOS.Setups;
using UIKit;

namespace Tokkepedia.iOS
{
    public partial class App
    {
        public static UIWindow Window { get; set; }
        private static ViewModelLocator _locator;
        public static ViewModelLocator Locator
        {
            get
            {
                if (_locator == null)
                {
                    // First time initialization
                    var nav = new NavigationService();
                    //nav.Initialize((UINavigationController)Window.RootViewController);
                    nav.Configure(
                        ViewModelLocator.LoginPageKey,
                        "LoginController");
                    nav.Configure(
                        ViewModelLocator.MainPageKey,
                        "MainController");
                    SimpleIoc.Default.Register<INavigationService>(() => nav);
                    SimpleIoc.Default.Register<IDialogService, DialogService>();
                    Window.MakeKeyAndVisible();

                    _locator = new ViewModelLocator();
                }

                return _locator;
            }
        }
    }
}