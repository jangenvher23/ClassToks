using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using ClassToksV2.Models;
using ClassToksV2.Views;
using ClassToksV2.ViewModels;
using ClassToksV2.Views.Tok.View;

namespace ClassToksV2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ToksPage : ContentPage
    {
        //ToksViewModel _viewModel;
        TokCardsFeed lstTokCards = new TokCardsFeed();

        ScrollView sv = new ScrollView();
        StackLayout cv = new StackLayout();
        TokCardView c = new TokCardView(), c2 = new TokCardView();

        public ToksPage()
        {
            InitializeComponent();

            //cv.VerticalOptions = LayoutOptions.FillAndExpand;
            //cv.Children.Add(c);
            //cv.Children.Add(c2);
            //sv.Content = cv;

            //cv.ItemsSource = new string[] { "1", "2" };
            //cv.ItemTemplate = new DataTemplate(() =>
            //{
            //    TokCardView cardView = new TokCardView();

            //    return cardView;
            //});

            GC.SuppressFinalize(lstTokTiles);

            BindingContext = App.Toks = new ToksViewModel() { TokParameterUIList = ItemsListView };
            //App.Toks = _viewModel;

            lstTokCards.BindingContext = App.Toks;

            App.Toks.OnAppearing();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        bool isCards = false;
        private async void btnFilter_Clicked(object sender, EventArgs e)
        {
            //if (isCards)
            //{
            //    this.Content = lstTokTiles;
            //    isCards = false;
            //}
            //else
            //{
            //    this.Content = lstTokCards;
            //    isCards = true;
            //}

            await Shell.Current.GoToAsync($"{nameof(FilterPage)}", true);

        }
    }
}