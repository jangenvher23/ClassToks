using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tokkepedia.iOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsNavigationController NavigationController;

        public SettingsPage()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty EnableBackButtonOverrideProperty =
                   BindableProperty.Create(
                   nameof(EnableBackButtonOverride),
                   typeof(bool),
                   typeof(SettingsPage),
                   false);

        /// <summary>
        /// Gets or Sets Custom Back button overriding state
        /// </summary>
        public bool EnableBackButtonOverride
        {
            get
            {
                return (bool)GetValue(EnableBackButtonOverrideProperty);
            }
            set
            {
                SetValue(EnableBackButtonOverrideProperty, value);
            }
        }

        private void btnLogOut_Tapped(object sender, EventArgs e)
        {
            NavigationController.LogOutUser();
        }

        private void btnAbout_Tapped(object sender, EventArgs e)
        {
            NavigationController.PopPage();
        }

        private void btnFaq_Tapped(object sender, EventArgs e)
        {
            var page = new TokCardsPage()
            {
                Title = "Tok Cards",
                //BindingContext = note
            };

            var iOSPage = (page).CreateViewController();

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
            UIApplication.SharedApplication.KeyWindow.RootViewController.HidesBottomBarWhenPushed = true; //= _navigation;

            this.NavigationController?.PushViewController(iOSPage, true);
        }
    }
}