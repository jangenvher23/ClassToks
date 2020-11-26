using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Tokkepedia.Shared.ViewModels;

namespace Tokkepedia.Shared.Setups
{
    public class ViewModelLocator
    {
        public const string LoginPageKey = "LoginPage";
        public const string MainPageKey = "MainPage";
        public const string SignupPageKey = "SignupPage";
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LoginPageViewModel>();

        }

        
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }
        public LoginPageViewModel LoginPageVM
        {
            get
            {
                return SimpleIoc.Default.GetInstance<LoginPageViewModel>();
            }
        }
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
