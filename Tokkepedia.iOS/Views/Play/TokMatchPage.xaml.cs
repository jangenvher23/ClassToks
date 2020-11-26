using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tokkepedia.iOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TokMatchPage : ContentPage
    {
        public SettingsNavigationController NavigationController;

        public TokMatchPage()
        {
            InitializeComponent();
        }

        private async void DropGestureRecognizer_DragOver(object sender, DragEventArgs e)
        {
            Debug.WriteLine(e.AcceptedOperation + ": " + e.Data.Text);
            //DisplayAlert("Answer", "Incorrect.", "OK");
        }

        private async void DropGestureRecognizer_Drop_1(object sender, DropEventArgs e)
        {
            await DisplayAlert("Answer", "Incorrect.", "OK");
        }

        private async void DropGestureRecognizer_Drop(object sender, DropEventArgs e)
        {
            await DisplayAlert("Answer", "Correct!", "OK");
        }
    }
}