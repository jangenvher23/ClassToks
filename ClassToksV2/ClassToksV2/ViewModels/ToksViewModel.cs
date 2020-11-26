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
using System.Collections.Generic;
using Newtonsoft.Json;
using Tokket.Tokkepedia.Tools;
using System.Linq;

namespace ClassToksV2.ViewModels
{
    public class ToksViewModel : BaseViewModel
    {
        public CollectionView TokParameterUIList { get; set; }

        List<string> Colors = new List<string>() {
               "#e57373","#f06292","#ba68c8","#9575cd",
        "#d32f2f", "#C2185B", "#7B1FA2", "#512DA8",
        "#7986cb", "#64b5f6", "#4fc3f7", "#4dd0e1",
        "#303F9F", "#1976D2", "#0288D1", "#0097A7",
        "#4db6ac", "#81c784", "#aed581", "#dce775",
        "#00796B", "#388E3C", "#689F38", "#AFB42B",
        "#fff176", "#ffd54f", "#ffb74d", "#ff8a65",
        "#FBC02D", "#FFA000", "#F57C00", "#E64A19"
               };
        List<string> randomcolors = new List<string>();

#if _TOKKEPEDIA
        private TokModel _selectedItem;
        public ObservableCollection<TokModel> Items { get; }
        public Command<TokModel> ItemTapped { get; }
#else
        private ClassTokXF _selectedItem;
        public ObservableCollection<ClassTokXF> Items { get; set; }
        public Command<ClassTokXF> ItemTapped { get; }
#endif

        private string ContinutationToken { get; set; } = null;

        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command AddTokCommand { get; }
        public Command ThresholdReachedCommand { get; }

        public ToksViewModel()
        {
            Title = "Toks";

#if _TOKKEPEDIA
            Items = new ObservableCollection<TokModel>();
            ItemTapped = new Command<TokModel>(OnItemSelected);
#else
            Items = new ObservableCollection<ClassTokXF>();
            ItemTapped = new Command<ClassTokXF>(OnItemSelected);
#endif

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            AddItemCommand = new Command(OnAddItem);
            AddTokCommand = new Command(OnAddTok);
            ThresholdReachedCommand = new Command(async () => await OnThresholdReached());
        }

        bool isLoading = false;
        async Task ExecuteLoadItemsCommand()
        {
            if (isLoading)
                return;
            isLoading = true;

            IsBusy = true;

            try
            {
                Items.Clear();

#if _TOKKEPEDIA
                var items = await TokService.Instance.GetToksAsync(new TokQueryValues() { });
#else
                var values = App.TokFilter;
                var items = await ClassService.Instance.GetClassToksAsync(values, new CancellationToken());
                ContinutationToken = items.ContinuationToken;
#endif
                RemainingItemsThreshold = (!string.IsNullOrEmpty(ContinutationToken)) ? 0 : -1;

                #region Process items
                var itemsXF = JsonConvert.DeserializeObject<ResultData<ClassTokXF>>(JsonConvert.SerializeObject(items));

                int i = 0, colorTotal = Colors.Count;
                Colors = RandomExtensions<string>.Randomize(Colors.ToArray()).ToList();
                foreach (var item in itemsXF.Results)
                {
                    item.ColorHex = Colors[i];
                    item.ColorXF = Color.FromHex(Colors[i]);
                    var processedItem = item.ProcessItem();

                    i = (i < Colors.Count - 1) ? i + 1 : 0;

                    Items.Add(processedItem);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;

                isLoading = false;
                FooterHeight = 0;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        async Task OnThresholdReached()
        {
            if (isLoading || RemainingItemsThreshold <= -1)
                return;

            isLoading = true;
            FooterHeight = 50;

            try
            {
#if _TOKKEPEDIA
                var items = await TokService.Instance.GetToksAsync(new TokQueryValues() { });
#else
                var values = new ClassTokQueryValues();
                if (!string.IsNullOrEmpty(ContinutationToken))
                    values.paginationid = ContinutationToken;

                var items = await ClassService.Instance.GetClassToksAsync(values, new CancellationToken());
                ContinutationToken = items.ContinuationToken;

                RemainingItemsThreshold = (!string.IsNullOrEmpty(ContinutationToken)) ? 0 : -1;
                ContinutationToken = (RemainingItemsThreshold < 0) ? null : ContinutationToken;
#endif

                #region Process items
                var itemsXF = JsonConvert.DeserializeObject<ResultData<ClassTokXF>>(JsonConvert.SerializeObject(items));

                int i = 0, colorTotal = Colors.Count;
                Colors = RandomExtensions<string>.Randomize(Colors.ToArray()).ToList();
                foreach (var item in itemsXF.Results)
                {
                    item.ColorHex = Colors[i];
                    item.ColorXF = Color.FromHex(Colors[i]);

                    var processedItem = item.ProcessItem();

                    //item.TileName = $"tile-{item.Id}";
                    i = (i < Colors.Count - 1) ? i + 1 : 0;

                    Items.Add(processedItem);
                }
                #endregion
                //MessagingCenter.Send<object, Item>(this, ScrollToPreviousLastItem, previousLastItem);
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


#if _TOKKEPEDIA
        public TokModel SelectedItem
#else
        public ClassTokXF SelectedItem
#endif
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
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

        private async void OnAddItem(object obj)
        {
            //await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        private async void OnAddTok(object obj)
        {
            //if (!App.CheckCredentials())
            //{
            //    await Application.Current.MainPage.DisplayAlert("Notice", "You must be logged in to add a tok.", "OK");
            //    return;
            //}
            //else
            //{
                
            //}
            App.TokParameter = null;
            await Shell.Current.GoToAsync(nameof(AddEditTokPage));
        }

#if _TOKKEPEDIA
        async void OnItemSelected(TokModel item)
#else
        async void OnItemSelected(ClassTokXF item)
#endif
        {
            if (item == null)
                return;

            App.TokParameter = item;
            App.TokParameterUIList = TokParameterUIList;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(TokInfoPage)}?{nameof(TokInfoViewModel.Id)}={item.Id}");
        }
    }
}