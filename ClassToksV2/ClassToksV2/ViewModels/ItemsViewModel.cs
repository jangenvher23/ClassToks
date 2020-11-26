//using System;
//using System.Collections.ObjectModel;
//using System.Diagnostics;
//using System.Threading.Tasks;

//using Xamarin.Forms;

//using ClassToks.Models;
//using ClassToks.Views;
//using Tokkepedia.Shared.Models;
//using Tokkepedia.Shared.Services;

//namespace ClassToks.ViewModels
//{
//    public class ItemsViewModel : BaseViewModel
//    {
//        private TokModel _selectedItem;

//        public ObservableCollection<TokModel> Items { get; }
//        public Command LoadItemsCommand { get; }
//        public Command AddItemCommand { get; }
//        public Command<TokModel> ItemTapped { get; }

//        public ItemsViewModel()
//        {
//            Title = "Toks";
//            Items = new ObservableCollection<TokModel>();
//            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

//            ItemTapped = new Command<TokModel>(OnItemSelected);

//            AddItemCommand = new Command(OnAddItem);
//        }

//        async Task ExecuteLoadItemsCommand()
//        {
//            IsBusy = true;

//            try
//            {
//                Items.Clear();
//                //var items = await DataStore.GetItemsAsync(true);
//                var items = await TokService.Instance.GetToksAsync(new TokQueryValues() { });
//                foreach (var item in items)
//                {
//                    Items.Add(item);
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex);
//            }
//            finally
//            {
//                IsBusy = false;
//            }
//        }

//        public void OnAppearing()
//        {
//            IsBusy = true;
//            SelectedItem = null;
//        }

//        public TokModel SelectedItem
//        {
//            get => _selectedItem;
//            set
//            {
//                SetProperty(ref _selectedItem, value);
//                OnItemSelected(value);
//            }
//        }

//        private async void OnAddItem(object obj)
//        {
//            await Shell.Current.GoToAsync(nameof(NewItemPage));
//        }

//        async void OnItemSelected(TokModel item)
//        {
//            if (item == null)
//                return;

//            // This will push the ItemDetailPage onto the navigation stack
//            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
//        }
//    }
//}