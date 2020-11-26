using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassToksV2.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClassToksV2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginSignUpPage : ContentPage
    {
        LoginSignUpViewModel ViewModel { get; set; }

        public LoginSignUpPage()
        {
            InitializeComponent();
            this.BindingContext = ViewModel = new LoginSignUpViewModel() { IsLoading = false };//Loader = waitingView
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.IsBusy = false;
        }

        private void btnGoToSignUp_Clicked(object sender, EventArgs e)
        {

        }

        private void btnGoToForgotPassword_Clicked(object sender, EventArgs e)
        {

        }

        private void pckAccountType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pckCountry_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}