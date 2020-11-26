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
    public partial class SetsPage : ContentPage
    {
        SetsViewModel _viewModel;

        public SetsPage()
        {
            InitializeComponent();

            BindingContext = App.Sets = new SetsViewModel();
            App.Sets.OnAppearing();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
        }
    }
}