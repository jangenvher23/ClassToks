using System.ComponentModel;
using Xamarin.Forms;
using ClassToksV2.ViewModels;

namespace ClassToksV2.Views
{
    public partial class GroupInfoPage : ContentPage
    {
        GroupInfoViewModel ViewModel { get; set; }
        public GroupInfoPage()
        {
            InitializeComponent();
            ViewModel = new GroupInfoViewModel() { Loader = waitingView };
            if (App.GroupParameter != null)
            {
                ViewModel.Item = App.GroupParameter;
            }

            BindingContext = ViewModel;
        }
    }
}