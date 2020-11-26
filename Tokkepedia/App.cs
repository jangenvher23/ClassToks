using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Tokkepedia.Setups;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.ViewModels;

namespace Tokkepedia
{
    public partial class App
    {
        private static ViewModelLocator _locator;
        public static ViewModelLocator Locator
        {
            get
            {
                if (_locator == null)
                {
                    // First time initialization
                    var nav = new NavigationService();
                    nav.Configure(
                        ViewModelLocator.LoginPageKey,
                        typeof(LoginActivity));
                    nav.Configure(
                        ViewModelLocator.MainPageKey,
                        typeof(MainActivity));
                    nav.Configure(
                        ViewModelLocator.SubAccountPage,
                        typeof(SubAccountActivity));
                    nav.Configure(
                        ViewModelLocator.MySetPageKey,
                        typeof(MySetsActivity));
                    nav.Configure(
                        ViewModelLocator.MySetsViewPageKey,
                        typeof(MySetsViewActivity));
                    nav.Configure(
                        ViewModelLocator.MyClassSetsViewPageKey,
                        typeof(MyClassSetsViewActivity));
                    nav.Configure(
                        ViewModelLocator.AddSetPageKey,
                        typeof(AddSetActivity));
                    nav.Configure(
                        ViewModelLocator.AddClassSetPageKey,
                        typeof(AddClassSetActivity));
                    nav.Configure(
                        ViewModelLocator.SignupPageKey,
                        typeof(SignupActivity));
                    
                    SimpleIoc.Default.Register<INavigationService>(() => nav);
                    SimpleIoc.Default.Register<IDialogService, DialogService>();

                    _locator = new ViewModelLocator();
                }

                return _locator;
            }
        }
    }
}