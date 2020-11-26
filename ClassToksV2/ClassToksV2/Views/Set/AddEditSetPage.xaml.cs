#define _CLASSTOKS
using ClassToksV2.Models;
using ClassToksV2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClassToksV2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddEditSetPage : ContentPage
    {
        int numDetails = 3;

        AddEditSetViewModel ViewModel { get; set; } = new AddEditSetViewModel();

        public AddEditSetPage()
        {
            InitializeComponent();
            ViewModel = new AddEditSetViewModel() { Loader = waitingView };

            if (App.SetParameter != null)
                ViewModel.Item = App.SetParameter;
            else
                ViewModel.Item = new ClassSetXF() { TokGroup = "Basic", IsAddMode = true };

            BindingContext = ViewModel;
            pckTokFormat.SelectedIndex = 0;
        }

        private void pckTokFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (sender as Picker).SelectedItem.ToString();
            ViewModel.Item.TokGroup = item;
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            //ClassTokXF newItem = new ClassTokXF()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    PrimaryFieldText = txtPrimary.Text,
            //    UserDisplayName = "Hello",
            //    ColorXF = Color.FromHex("#d32f2f")
            //};
            //App.Toks.Items.Insert(0, Item);
            //await DataStore.AddItemAsync(newItem);


            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }
    }
}