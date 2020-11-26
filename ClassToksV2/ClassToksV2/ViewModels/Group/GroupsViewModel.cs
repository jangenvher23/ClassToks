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
using Newtonsoft.Json;
using Tokket.Tokkepedia.Tools;

namespace ClassToksV2.ViewModels
{
    public class GroupsViewModel : BaseViewModel
    {
        private string ContinutationToken { get; set; } = null;

#if _TOKKEPEDIA
        private GroupModel _selectedItem;
        public ObservableCollection<GroupModel> Items { get; }
        public Command<GroupModel> ItemTapped { get; }
#else
        private ClassGroupXF _selectedItem;
        public ObservableCollection<ClassGroupXF> Items { get; }
        public Command<ClassGroupXF> ItemTapped { get; }
#endif

        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }

        public GroupsViewModel()
        {
            Title = "Groups";

#if _TOKKEPEDIA
            Items = new ObservableCollection<GroupModel>();
            ItemTapped = new Command<GroupModel>(OnItemSelected);
#else
            Items = new ObservableCollection<ClassGroupXF>();
            ItemTapped = new Command<ClassGroupXF>(OnItemSelected);
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
                var items = await GroupService.Instance.GetGroupsAsync(new GroupQueryValues() { });
#else
                var values = new ClassGroupQueryValues();
                var items = await ClassService.Instance.GetClassGroupAsync(values);
#endif
                var itemsXF = JsonConvert.DeserializeObject<ResultData<ClassGroupXF>>(JsonConvert.SerializeObject(items));
                foreach (var item in itemsXF.Results)
                {
                    ProcessItem(item);
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

                isLoading = true;
                FooterHeight = 50;
            }
        }

        ClassGroupXF ProcessItem(ClassGroupXF item)
        {
            item.HasImage = !string.IsNullOrEmpty(item.Image) ? true : false;
            return item;
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

#if _TOKKEPEDIA
        public GroupModel SelectedItem
#else
        public ClassGroupXF SelectedItem
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
            App.GroupParameter = null;
            await Shell.Current.GoToAsync(nameof(AddEditGroupPage));
        }

        async void OnItemSelected(ClassGroupXF item)
        {
            if (item == null)
                return;

            App.GroupParameter = item;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(GroupInfoPage)}?{nameof(GroupInfoViewModel.Id)}={item.Id}");
        }

        private int _footerHeight = 0;
        public int FooterHeight
        {
            get { return _footerHeight; }
            set { SetProperty(ref _footerHeight, value); }
        }

        private int _itemThreshold = 0;
        public int RemainingItemsThreshold
        {
            get { return _itemThreshold; }
            set { SetProperty(ref _itemThreshold, value); }
        }



        bool isLoading = false;
        async Task OnThresholdReached()
        {
            if (isLoading || RemainingItemsThreshold <= -1)
                return;

            isLoading = true;
            FooterHeight = 50;

            try
            {
                var values = new ClassGroupQueryValues();
                if (!string.IsNullOrEmpty(ContinutationToken))
                    values.paginationid = ContinutationToken;

                var items = await ClassService.Instance.GetClassGroupAsync(values);

                var itemsXF = JsonConvert.DeserializeObject<ResultData<ClassGroupXF>>(JsonConvert.SerializeObject(items));
                foreach (var item in itemsXF.Results)
                {
                    ProcessItem(item);
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                isLoading = false;
                FooterHeight = 0;
            }
        }

    }
}