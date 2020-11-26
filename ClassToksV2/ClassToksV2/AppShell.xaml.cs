using System;
using System.Collections.Generic;
using ClassToksV2.ViewModels;
using ClassToksV2.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ClassToksV2
{
    //Source="https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/shell/flyout-images/flyout-header.png"
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            //Do not move to OnAppearing: 'unable to figure out route for: 
            //Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            //Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
            Routing.RegisterRoute(nameof(AddEditTokPage), typeof(AddEditTokPage));
            Routing.RegisterRoute(nameof(TokInfoPage), typeof(TokInfoPage));
            Routing.RegisterRoute(nameof(GroupInfoPage), typeof(GroupInfoPage));
            Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
            Routing.RegisterRoute(nameof(TokCardsPage), typeof(TokCardsPage));
            Routing.RegisterRoute(nameof(TokMatchPage), typeof(TokMatchPage));
            Routing.RegisterRoute(nameof(TokBackPage), typeof(TokBackPage));
            Routing.RegisterRoute(nameof(FilterPage), typeof(FilterPage));
            Routing.RegisterRoute(nameof(LoginSignUpPage), typeof(LoginSignUpPage));
            Routing.RegisterRoute(nameof(SetsPage), typeof(SetsPage));
            Routing.RegisterRoute(nameof(GroupsPage), typeof(GroupsPage));
            Routing.RegisterRoute(nameof(AddEditSetPage), typeof(AddEditSetPage));
            Routing.RegisterRoute(nameof(AddEditGroupPage), typeof(AddEditGroupPage));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            SecureStorage.RemoveAll();

            Shell.Current.FlyoutIsPresented = false;

            if ((sender as MenuItem).Text == "Class Sets")
            {
                await Shell.Current.GoToAsync($"{nameof(SetsPage)}");
            }
            else if ((sender as MenuItem).Text == "Class Groups")
            {
                await Shell.Current.GoToAsync($"{nameof(GroupsPage)}");
            }
            else
            {
                Shell.SetTabBarIsVisible(Shell.Current, false);
                await Shell.Current.GoToAsync($"{nameof(LoginSignUpPage)}");
            }

            //await Shell.Current.GoToAsync($"{nameof(AboutPage)}");
            //await Shell.Current.GoToAsync($"{nameof(TokBackPage)}");
        }
    }
}
