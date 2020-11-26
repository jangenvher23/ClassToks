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

namespace ClassToksV2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ToksCardsPage : ContentPage
    {
        ToksViewModel _viewModel;

        public ToksCardsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ToksViewModel();
            App.Toks = _viewModel;

            _viewModel.OnAppearing();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
        }
    }
}