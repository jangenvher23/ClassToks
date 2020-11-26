


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tokkepedia.iOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetsPage : ContentPage
    {
        public SetsNavigationController NavigationController;

        public ResultData<Set> SetsData { get; set; }
        public List<Set> Sets { get; set; }

        public SetsPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var setQueryValues = new SetQueryValues() { userid = Settings.GetUserModel().UserId };
            SetsData = await SetService.Instance.GetSetsAsync(null);
            Sets = SetsData.Results.ToList();

            lstSets.SetBinding(ItemsView.ItemsSourceProperty, "Sets");
        }

        private void btnTokCards_Clicked(object sender, EventArgs e)
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

        private void btnTokMatch_Clicked(object sender, EventArgs e)
        {
            var page = new TokMatchPage()
            {
                Title = "Tok Match",
                //BindingContext = note
            };

            var iOSPage = (page).CreateViewController();

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
            UIApplication.SharedApplication.KeyWindow.RootViewController.HidesBottomBarWhenPushed = true; //= _navigation;

            this.NavigationController?.PushViewController(iOSPage, true);
        }

        private void btnTokChoice_Clicked(object sender, EventArgs e)
        {
            var page = new TokCardsPage()
            {
                Title = "Tok Choice",
                //BindingContext = note
            };

            var iOSPage = (page).CreateViewController();

            UIApplication.SharedApplication.KeyWindow.MakeKeyAndVisible();
            UIApplication.SharedApplication.KeyWindow.RootViewController.HidesBottomBarWhenPushed = true; //= _navigation;

            this.NavigationController?.PushViewController(iOSPage, true);
        }

        bool running = false;
        private async void lstSetsRefresh_Refreshing(object sender, EventArgs e)
        {
            //if (running == faf)
            //lstSetsRefresh.IsRefreshing = true;
            //SetsData = await SetService.Instance.GetSetsAsync(null);
            //Sets = SetsData.Results.ToList();
            //lstSetsRefresh.IsRefreshing = false;
        }

        private async void lstSets_RemainingItemsThresholdReached(object sender, EventArgs e)
        {

            //lstSetsRefresh.IsRefreshing = true;
            var setQueryValues = new SetQueryValues() { token = SetsData?.ContinuationToken };
            SetsData = await SetService.Instance.GetSetsAsync(setQueryValues);
            Sets.AddRange(SetsData.Results);

            //lstSets.SetBinding(ItemsView.ItemsSourceProperty, "Sets");
            //lstSetsRefresh.IsRefreshing = false;
        }
    }


}