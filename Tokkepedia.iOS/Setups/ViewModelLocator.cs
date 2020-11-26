using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonServiceLocator;
using Foundation;
using GalaSoft.MvvmLight.Ioc;
using Tokkepedia.iOS.ViewModels;
using UIKit;

namespace Tokkepedia.iOS.Setups
{
    public class ViewModelLocator
    {
        public const string LoginPageKey = "LoginPage";
        public const string MainPageKey = "MainPage";
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<LoginPageViewModel>();
            SimpleIoc.Default.Register<MainPageViewModel>();
        }
        public LoginPageViewModel LoginPageVM
        {
            get
            {
                return SimpleIoc.Default.GetInstance<LoginPageViewModel>();
            }
        }
        public MainPageViewModel MainPageVM
        {
            get
            {
                return SimpleIoc.Default.GetInstance<MainPageViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}