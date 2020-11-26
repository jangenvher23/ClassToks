using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using ClassToksV2.Models;
using ClassToksV2.Views;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using System.Threading;

namespace ClassToksV2.ViewModels
{
    public class SetsViewModel : BaseViewModel
    {


#if _TOKKEPEDIA
        private SetModel _selectedItem;
        public ObservableCollection<SetModel> Items { get; }
        public Command<SetModel> ItemTapped { get; }
#else
        private ClassSetModel _selectedItem;
        public ObservableCollection<ClassSetModel> Items { get; }
        public Command<ClassSetModel> ItemTapped { get; }
#endif

        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }

        public SetsViewModel()
        {
            Title = "Sets";

#if _TOKKEPEDIA
            Items = new ObservableCollection<SetModel>();
            ItemTapped = new Command<SetModel>(OnItemSelected);
#else
            Items = new ObservableCollection<ClassSetModel>();
            ItemTapped = new Command<ClassSetModel>(OnItemSelected);
#endif

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            AddItemCommand = new Command(OnAddItem);
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();

#if _TOKKEPEDIA
                var items = await SetService.Instance.GetSetsAsync(new SetQueryValues() { });
#else
                var values = new ClassSetQueryValues();
                var items = await ClassService.Instance.GetClassSetAsync(values);
#endif

                foreach (var item in items.Results)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

#if _TOKKEPEDIA
        public SetModel SelectedItem
#else
        public ClassSetModel SelectedItem
#endif
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private async void OnAddItem(object obj)
        {
            
            await Shell.Current.GoToAsync(nameof(AddEditSetPage));
        }

#if _TOKKEPEDIA
        async void OnItemSelected(SetModel item)
#else
        async void OnItemSelected(ClassSetModel item)
#endif
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            //await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
        }
    }
}