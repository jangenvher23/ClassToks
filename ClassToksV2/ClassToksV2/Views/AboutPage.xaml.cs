using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClassToksV2.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private async void btnTokCards_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(TokCardsPage)}");
        }

        private async void btnTokMatch_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(TokMatchPage)}");
        }

        private async void btnTokBack_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(TokBackPage)}");
        }
    }
}