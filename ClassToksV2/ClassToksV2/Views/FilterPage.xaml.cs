
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClassToksV2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FilterPage : ContentPage
    {
        public FilterPage()
        {
            InitializeComponent();
        }

        string userid = "1tULByrYkkdXQzvRVR3EZqavKZh1";
        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("..");
        }

        private void btnApply_Clicked(object sender, EventArgs e)
        {
            App.Toks.OnAppearing();
            Shell.Current.GoToAsync("..");
        }

        private void btnPublicClassToks_Clicked(object sender, EventArgs e)
        {
            App.TokFilter.userid = null;
            btnPublicClassToks.BackgroundColor = Color.Purple;

            btnMyClassToks.BackgroundColor = Color.FromHex("#3F51B5");
        }

        private void btnMyClassToks_Clicked(object sender, EventArgs e)
        {
            App.TokFilter.userid = userid;
            btnMyClassToks.BackgroundColor = Color.Purple;

            btnPublicClassToks.BackgroundColor = Color.FromHex("#3F51B5");
        }

        //protected override bool OnBackgroundClicked()
        //{
        //    Shell.Current.GoToAsync("..");
        //    return base.OnBackgroundClicked();
        //}
    }
}