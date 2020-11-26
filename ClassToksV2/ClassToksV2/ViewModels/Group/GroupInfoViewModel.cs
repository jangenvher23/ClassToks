using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClassToksV2.Models;
using ClassToksV2.Views;
using Tokkepedia.Shared.Services;
using Tokkepedia.Shared.Models;
using Xamarin.Forms;

namespace ClassToksV2.ViewModels
{
    [QueryProperty(nameof(Id), nameof(Id))]
    public class GroupInfoViewModel : BaseViewModel
    {
        private string itemId;
        private string text;
        private string description;
        public string Id { get; set; }

        public ClassGroupXF Item { get; set; }

        //Section
        public TokSectionsViewModel Sections { get; set; }

        bool isOwner = true;
        public WaitingView Loader { get; set; }

        public GroupInfoViewModel()
        {
            ActionsTokCommand = new Command(OnActionsTok);
        }

        public Command ActionsTokCommand { get; }

#if _TOKKEPEDIA
        async void OnItemSelected(TokModel item)
#else
        async void OnActionsTok()//ClassTokModel item
#endif
        {
            App.GroupParameter = Item;
            //if (item == null)
            //    return;

            string selected = "";
            if (isOwner)
                selected = await App.Current.MainPage.DisplayActionSheet("Actions", "Cancel", null, "Edit", "Delete", "Share");
            else
                selected = await App.Current.MainPage.DisplayActionSheet("Actions", "Cancel", null, "Report Bad Group", "Share");

            if (selected == "Edit")
            {
                App.GroupParameter.IsAddMode = false;
                await Shell.Current.GoToAsync(nameof(AddEditGroupPage));
            }
            else if (selected == "Delete")
            {
                try
                {
                    var accept = await Application.Current.MainPage.DisplayAlert("Confirm", "Are you sure you want to delete this?", "Delete", "Cancel");

                    if (accept)
                    {
                        Loader.IsVisible = true;

                        await ClassService.Instance.DeleteClassGroupAsync(Item.Id, Item.PartitionKey);
                        App.Groups.Items.Remove(App.GroupParameter);// TODO: don't allow screen to be touched until tok removed. Does not like the top tab switching

                        Loader.IsVisible = false;

                        await Shell.Current.GoToAsync("../..");
                    }
                }
                catch (Exception ex) { }
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            //SelectedItem = null;
        }

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        public string ItemId
        {
            get
            {
                return itemId;
            }
            set
            {
                itemId = value;
                LoadItemId(value);
            }
        }

        //TODO: Does not work
        public void LoadItemId(string itemId)
        {
            try
            {
                var item = App.Toks.Items.FirstOrDefault(x => x.Id == itemId);//await DataStore.GetItemAsync(itemId);
                Id = item.Id;
                Text = item.PrimaryFieldText;
                Description = item.UserDisplayName;
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }
    }
}
