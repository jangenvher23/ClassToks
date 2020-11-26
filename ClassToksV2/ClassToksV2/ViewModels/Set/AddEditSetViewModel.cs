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
    public class AddEditSetViewModel : BaseViewModel
    {
        public ClassSetXF Item { get; set; }
        public WaitingView Loader { get; set; }

        public AddEditSetViewModel()
        {
            AddEditSetCommand = new Command(OnAddEditSet);
        }

        public Command AddEditSetCommand { get; }

        async void OnAddEditSet()
        {
            if (!IsBusy)
            {
                IsBusy = true;
                Loader.IsVisible = true;

                try
                {
                    #region Get from text entries
                    Item.UserId = Settings.GetUserModel().UserId;
                    Item.Label = "classset";

                    #endregion

                    //Fields
                    Item.TokTypeId = $"toktype-{Item.TokGroup?.ToIdFormat()}-{Item.TokType?.ToIdFormat()}";

                    //User
                    //User Data
                    Item.UserDisplayName = Settings.GetTokketUser().DisplayName;
                    Item.UserId = Settings.GetTokketUser().Id;
                    Item.UserCountry = Settings.GetTokketUser().Country;
                    Item.UserPhoto = Settings.GetTokketUser().UserPhoto;

                    //API
                    if (Item.IsAddMode)
                    {
                        await ClassService.Instance.AddClassSetAsync(Item);
                        App.Sets.Items.Insert(0, Item);
                    }
                    else
                    {
                        await ClassService.Instance.UpdateClassSetAsync(Item);
                    }

                    Item.ColorMainHex = "#FFFFFF";
                    Item.ColorXF = Color.FromHex("#FFFFFF");

                    App.SetParameter = Item;
                    await Shell.Current.GoToAsync("..");
                }
                catch (Exception ex)
                {

                }

                Loader.IsVisible = false;
                IsBusy = false;
            }

        }
    }
}
