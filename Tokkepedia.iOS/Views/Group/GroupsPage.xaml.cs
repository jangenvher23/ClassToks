using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tokkepedia.iOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GroupsPage : ContentPage
    {
        public SetsNavigationController NavigationController;

        public ResultData<ClassGroupModel> GroupsData { get; set; }
        public List<ClassGroupModel> Groups { get; set; }

        public GroupsPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var groupQueryValues = new ClassGroupQueryValues() { userid = Settings.GetUserModel().UserId };
        
            GroupsData = await ClassService.Instance.GetClassGroupRequestAsync(null);
            Groups = GroupsData.Results.ToList();

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
            var queryValues = new ClassGroupQueryValues() { paginationid = GroupsData?.ContinuationToken };
            GroupsData = await ClassService.Instance.GetClassGroupAsync(queryValues);
            Groups.AddRange(GroupsData.Results);

            //lstSets.SetBinding(ItemsView.ItemsSourceProperty, "Sets");
            //lstSetsRefresh.IsRefreshing = false;
        }
    }


}