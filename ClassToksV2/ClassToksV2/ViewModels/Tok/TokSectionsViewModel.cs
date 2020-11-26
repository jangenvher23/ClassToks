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
using Tokket.Tokkepedia;

namespace ClassToksV2.ViewModels
{
    public class TokSectionsViewModel : BaseViewModel
    {
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
        private TokSectionXF _selectedItem;
        public ObservableCollection<TokSectionXF> Items { get; }
        public Command<TokSectionXF> ItemTapped { get; }
#endif

        public string ContinuationToken { get; set; } = null;
        public string TokId { get; set; } = null;

        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command AddTokCommand { get; }
        public Command ThresholdReachedCommand { get; }

        public TokSectionsViewModel()
        {
            Title = "Toks";

#if _TOKKEPEDIA
            Items = new ObservableCollection<TokModel>();
            ItemTapped = new Command<TokModel>(OnItemSelected);
#else
            Items = new ObservableCollection<TokSectionXF>();
            ItemTapped = new Command<TokSectionXF>(OnItemSelected);
#endif

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            AddItemCommand = new Command(OnAddItem);
            AddTokCommand = new Command(OnAddTok);
            ThresholdReachedCommand = new Command(async () => await OnThresholdReached());
        }

        bool isLoading = false;
        public async Task ExecuteLoadItemsCommand()
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
                //var values = App.TokFilter;
                var items = await TokService.Instance.GetTokSectionsAsync(TokId);
                ContinuationToken = items.ContinuationToken;
#endif
                RemainingItemsThreshold = (!string.IsNullOrEmpty(ContinuationToken)) ? 0 : -1;

                #region Process items
                var itemsXF = JsonConvert.DeserializeObject<ResultData<TokSectionXF>>(JsonConvert.SerializeObject(items));

                foreach (var item in itemsXF.Results)
                {
                    var processedItem = ProcessItem(item);
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

        public async Task OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;

            ExecuteLoadItemsCommand();
        }

        TokSectionXF ProcessItem(TokSectionXF item)
        {
            //item.IsBasic = item.TokGroup == "Basic" || (!item.IsDetailBased && !item.IsMega) ? true : false;
            //item.IsDetailed = item.TokGroup == "Detailed" || (item.IsDetailBased && !item.IsMega) ? true : false;
            //item.IsSectioned = item.TokGroup == "Mega" || (!item.IsDetailBased && item.IsMega) ? true : false;

            ////Details
            //if (item.IsDetailed)
            //{
            //    if (!item.IsEnglish)
            //    {
            //        item.Detail1 = item.Details[0];
            //        item.Detail2 = item.Details[1];

            //        item.Detail3 = item.EnglishPrimaryFieldText;
            //        item.Detail4 = item.EnglishDetails[0];
            //        item.HasDetail4 = !string.IsNullOrEmpty(item.Detail4) ? true : false;
            //    }
            //    else
            //    {
            //        item.Detail1 = item.Details[0];
            //        item.Detail2 = item.Details[1];
            //        item.Detail3 = item.Details[2];
            //        item.Detail4 = item.Details[3];
            //        item.HasDetail4 = !string.IsNullOrEmpty(item.Detail4) ? true : false;
            //    }

            //    int detailCount = item.Details.Where(x => !string.IsNullOrEmpty(x)).Count();
            //    for (int j = 0; j < detailCount; ++j)
            //    {
            //        item.Details[j] = $"- {item.Details[j]}";
            //    }
            //    item.ViewMore = $"View {detailCount} Details";
            //}
            //else if (item.IsMega)
            //{
            //    item.SectionTitle1 = "- " + item.SectionTitles[0];
            //    item.SectionTitle2 = "- " + item.SectionTitles[1];
            //    item.HasSectionTitle2 = !string.IsNullOrEmpty(item.SectionTitle2) ? true : false;
            //    item.SectionTitle3 = "- " + item.SectionTitles[2];
            //    item.HasSectionTitle3 = !string.IsNullOrEmpty(item.SectionTitle3) ? true : false;
            //    item.SectionTitle4 = "- " + item.SectionTitles[3];
            //    item.HasSectionTitle4 = !string.IsNullOrEmpty(item.SectionTitle4) ? true : false;

            //    item.ViewMore = $"View {item.SectionCount} Sections";
            //}

            //item.HasImage = !string.IsNullOrEmpty(item.Image) ? true : false;
            //if (item.HasImage)
            //{
            //    item.IsDetailed = false;
            //    item.IsMega = false;

            //    item.FontAttributesTitleSubaccount = FontAttributes.Bold;
            //    item.FontAttributesPrimary = FontAttributes.None;
            //}


            return item;
        }


        public async Task OnThresholdReached()
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
                //var values = App.TokFilter;
                var items = await TokService.Instance.GetTokSectionsAsync(TokId);
                ContinuationToken = items.ContinuationToken;
#endif
                RemainingItemsThreshold = (!string.IsNullOrEmpty(ContinuationToken)) ? 0 : -1;

                #region Process items
                var itemsXF = JsonConvert.DeserializeObject<ResultData<TokSectionXF>>(JsonConvert.SerializeObject(items));

                foreach (var item in itemsXF.Results)
                {
                    var processedItem = ProcessItem(item);
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
                isLoading = false;
                FooterHeight = 0;
            }
        }


#if _TOKKEPEDIA
        public TokModel SelectedItem
#else
        public TokSectionXF SelectedItem
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
           // await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        private async void OnAddTok(object obj)
        {
            App.TokParameter = null;
            await Shell.Current.GoToAsync(nameof(AddEditTokPage));
        }

#if _TOKKEPEDIA
        async void OnItemSelected(TokModel item)
#else
        async void OnItemSelected(TokSectionXF item)
#endif
        {
            if (item == null)
                return;

            App.TokSectionParameter = item;

            // This will push the ItemDetailPage onto the navigation stack
            //await Shell.Current.GoToAsync($"{nameof(TokInfoPage)}?{nameof(TokInfoViewModel.Id)}={item.Id}");
        }
    }
}
