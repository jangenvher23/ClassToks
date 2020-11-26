using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClassToksV2.Models;
using ClassToksV2.Views;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia.Tools;
using Xamarin.Forms;

namespace ClassToksV2.ViewModels
{
    public class AddEditGroupViewModel : BaseViewModel
    {
        public ClassGroupXF Item { get; set; }
        public WaitingView Loader { get; set; }

        public AddEditGroupViewModel()
        {
            AddEditGroupCommand = new Command(OnAddEditGroup);
        }

        public Command AddEditGroupCommand { get; }

        async void OnAddEditGroup()
        {
            if (!IsBusy)
            {
                IsBusy = true;
                Loader.IsVisible = true;
                IsLoaderVisible = true;

                try
                {
                    #region Get from text entries
                    Item.UserId = "1tULByrYkkdXQzvRVR3EZqavKZh1"; //Settings.GetUserModel().UserId;
                    Item.Label = "classtok";

                    #endregion

                    //Fields
                    //Item.CategoryId = $"category-{Item.Category?.ToIdFormat()}";
                    //Item.TokTypeId = $"toktype-{Item.TokGroup?.ToIdFormat()}-{Item.TokType?.ToIdFormat()}";

                    //User
                    //User Data
                    Item.UserDisplayName = Settings.GetTokketUser().DisplayName;
                    Item.UserId = Settings.GetTokketUser().Id;
                    Item.UserCountry = Settings.GetTokketUser().Country;
                    Item.UserPhoto = Settings.GetTokketUser().UserPhoto;

                    //API
                    if (Item.IsAddMode)
                    {
                        await ClassService.Instance.AddClassGroupAsync(Item);
                        App.Groups.Items.Insert(0, Item);
                    }
                    else
                    {
                        await ClassService.Instance.UpdateClassGroupAsync(Item);
                    }

                    Item.ColorMainHex = "#FFFFFF";
                    Item.ColorXF = Color.FromHex("#FFFFFF");

                    App.GroupParameter = Item;
                    await Shell.Current.GoToAsync("..");
                }
                catch (Exception ex)
                {

                }

                IsLoaderVisible = false;
                Loader.IsVisible = false;
                IsBusy = false;
            }

        }

        #region Properties
        private string _toolbarText = "Add Group";
        public string ToolbarText
        {
            get { return _toolbarText; }
            set { SetProperty(ref _toolbarText, value); }
        }

        private bool _isLoaderVisible = false;
        public bool IsLoaderVisible
        {
            get { return _isLoaderVisible; }
            set { SetProperty(ref _isLoaderVisible, value); }
        }
        #endregion
    }
}
