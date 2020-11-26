using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Preference;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Newtonsoft.Json;
using Supercharge;
using Tokkepedia.Adapters;
using Tokkepedia.Listener;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokkepedia.Shared.ViewModels;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using Xamarin.Essentials;
using Color = Android.Graphics.Color;

namespace Tokkepedia.Fragments
{
    public class classtoks_fragment : AndroidX.Fragment.App.Fragment
    {
        internal static classtoks_fragment Instance { get; private set; }
        View v; ClassTokModel classtokModel;
        GridLayoutManager mLayoutManager; 
        ClassTokDataAdapter ClassTokDataAdapter; ClassTokCardDataAdapter classtokcardDataAdapter;
        List<ClassTokModel> ClassTokCollection; List<Tokmoji> ListTokmojiModel;
        string groupId = "", classSetId = ""; ClassSetViewModel ClassSetVM;
        public bool isSearchFragment { get; set; } = false;
        public string filter = "";
        public string filterText { get; set; } = "";
        public FilterType filterType { get; set; } = FilterType.All;
        FilterBy filterBy = FilterBy.None;
        List<string> filterItems = new List<string>();
        public classtoks_fragment(string _groupId, string _classSetId = "", string _ClassSetVM = "")
        {
            groupId = _groupId;
            classSetId = _classSetId;
            ClassSetVM = JsonConvert.DeserializeObject<ClassSetViewModel>(_ClassSetVM);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.home_page, container, false);
            Instance = this;

            //Acr.UserDialogs.UserDialogs.Instance.HideLoading();

            if (groupId == "")
            {
                TextNothingFound.Visibility = ViewStates.Gone;
                ClassTokIconImg.Visibility = ViewStates.Visible;
            }
            else
            {
                TextNothingFound.Visibility = ViewStates.Visible;
                ClassTokIconImg.Visibility = ViewStates.Gone;
            }

            if (string.IsNullOrEmpty(TextNothingFound.Text)) TextNothingFound.Visibility = ViewStates.Gone;

            int numcol = 1;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                numcol = (int)NumDisplay.tablet + 1;
            }

            mLayoutManager = new GridLayoutManager(Application.Context, numcol);
            TaskRecyclerView.SetLayoutManager(mLayoutManager);

            ((Activity)Context).RunOnUiThread(async () => await InitializeData());
            
            RefreshLayout.SetColorSchemeColors(Color.ParseColor("#3498db"), Color.ParseColor("#4ea4de"), Color.ParseColor("#68aede"), Color.ParseColor("#88bee3"));
            RefreshLayout.Refresh -= RefreshLayout_Refresh;
            RefreshLayout.Refresh += RefreshLayout_Refresh;

            if (TaskRecyclerView != null)
            {
                TaskRecyclerView.HasFixedSize = true;

                var onScrollListener = new XamarinRecyclerViewOnScrollListener(mLayoutManager);
                onScrollListener.LoadMoreEvent += async (object sender, EventArgs e) =>
                {
                    await LoadMoreData();
                };

                TaskRecyclerView.AddOnScrollListener(onScrollListener);

                TaskRecyclerView.SetLayoutManager(mLayoutManager);
            }

            return v;
        }
        private void showBottomLoading()
        {
            progressbar.IndeterminateDrawable.SetColorFilter(new Color(ContextCompat.GetColor(v.Context, Resource.Color.colorAccent)), PorterDuff.Mode.Multiply);
            progressbar.Visibility = ViewStates.Visible;
        }
        private void hideBottomLoading()
        {
            progressbar.Visibility = ViewStates.Gone;
        }
        public async Task InitializeData()
        {
            ClassTokCollection = new List<ClassTokModel>();
            ClassTokCollection.Clear();
            TaskRecyclerView.SetAdapter(null);

            ShimmerLayout.StartShimmerAnimation();
            ShimmerLayout.Visibility = ViewStates.Visible;

            /*for (int i = 0; i < 200000; i++)
            {
                if (i % 20000 == 0)
                    source.Token.ThrowIfCancellationRequested();
            }*/

            //get tokmojis
            var tokmojiResult = await TokMojiService.Instance.GetTokmojisAsync(null);
            ListTokmojiModel = tokmojiResult.Results.ToList();

            List<ClassTokModel> resultToksData;

            if (!string.IsNullOrEmpty(classSetId))
            {
                resultToksData = ClassSetVM.ClassToks;
            }
            else
            {
                resultToksData = await GetClassToksData();
            }

            if (resultToksData != null)
            {
                foreach (var item in resultToksData)
                {
                    if (item.IsMegaTok == true || item.TokGroup.ToLower() == "mega")
                    {
                        bool isGetSection = false;
                        if (isSearchFragment)
                        {
                            if (Settings.FilterToksSearch == (int)FilterToks.Cards)
                            {
                                isGetSection = true;
                            }
                        }
                        else
                        {
                            if (Settings.FilterToksHome == (int)FilterToks.Cards)
                            {
                                isGetSection = true;
                            }
                        }

                        if (isGetSection)
                        {
                            var getTokSectionsResult = await TokService.Instance.GetTokSectionsAsync(item.Id);
                            var getTokSections = getTokSectionsResult.Results;
                            item.Sections = getTokSections.ToArray();
                        }
                    }

                    ClassTokCollection.Add(item);
                }

                if (isSearchFragment)
                {
                    //FILTER only shows the "View As" option if the Toks tab is selected
                    Settings.ActivityInt = (int)ActivityType.TokSearch;

                    if (ClassTokCollection.Count == 0)
                    {
                        TextNothingFound.Text = "No class toks.";
                    }
                    else
                    {
                        TextNothingFound.Visibility = ViewStates.Gone;
                    }
                }

                if (isSearchFragment)
                {
                    if (Settings.FilterToksSearch == (int)FilterToks.Toks)
                    {
                        SetClassTokRecyclerAdapter();
                    }
                    else if (Settings.FilterToksSearch == (int)FilterToks.Cards)
                    {
                        SetClassCardsRecyclerAdapter();
                    }
                }
                else
                {
                    if (Settings.FilterToksHome == (int)FilterToks.Toks)
                    {
                        SetClassTokRecyclerAdapter();
                    }
                    else if (Settings.FilterToksHome == (int)FilterToks.Cards)
                    {
                        SetClassCardsRecyclerAdapter();
                    }
                }
            }

            ShimmerLayout.Visibility = ViewStates.Gone;
        }
        public void AddClassTokCollection(ClassTokModel classTokItem)
        {
            int ndx = 0;
            bool isEdit = false;
            var collection = ClassTokCollection.FirstOrDefault(a => a.Id == classTokItem.Id);
            if (collection != null) //If item exist
            {
                ndx = ClassTokCollection.IndexOf(collection); //Get index
                ClassTokCollection.Remove(collection);
                ClassTokCollection.Insert(ndx, classTokItem);
                isEdit = true;
            }
            else
            {
                ClassTokCollection.Insert(0, classTokItem);
            }

            if (isSearchFragment)
            {
                if (Settings.FilterToksSearch == (int)FilterToks.Toks)
                {
                    SetClassTokRecyclerAdapter();
                }
                else if (Settings.FilterToksSearch == (int)FilterToks.Cards)
                {
                    SetClassCardsRecyclerAdapter();
                }
            }
            else
            {
                if (Settings.FilterToksHome == (int)FilterToks.Toks)
                {
                    SetClassTokRecyclerAdapter();
                }
                else if (Settings.FilterToksHome == (int)FilterToks.Cards)
                {
                    SetClassCardsRecyclerAdapter();
                }
            }


            if (isEdit)
            {
                TaskRecyclerView.ScrollToPosition(ndx);
            }
        }
        private void SetClassTokRecyclerAdapter(List<ClassTokModel> loadMoreItems = null)
        {
            if (loadMoreItems == null)
            {
                ClassTokDataAdapter = new ClassTokDataAdapter(ClassTokCollection, ListTokmojiModel);
                ClassTokDataAdapter.ItemClick -= OnGridBackgroundClick;
                ClassTokDataAdapter.ItemClick += OnGridBackgroundClick;
                TaskRecyclerView.SetAdapter(ClassTokDataAdapter);
            }
            else
            {
                ClassTokDataAdapter.UpdateItems(loadMoreItems, TaskRecyclerView.ChildCount);
                TaskRecyclerView.SetAdapter(ClassTokDataAdapter);
                TaskRecyclerView.ScrollToPosition(ClassTokDataAdapter.ItemCount - loadMoreItems.Count);
            }
        }
        private void SetClassCardsRecyclerAdapter(List<ClassTokModel> loadMoreItems = null)
        {
            if (loadMoreItems == null)
            {
                classtokcardDataAdapter = new ClassTokCardDataAdapter(ClassTokCollection, ListTokmojiModel);
                classtokcardDataAdapter.ItemClick += OnGridBackgroundClick;
                TaskRecyclerView.SetAdapter(classtokcardDataAdapter);
            }
            else
            {
                classtokcardDataAdapter.UpdateItems(loadMoreItems, TaskRecyclerView.ChildCount);
                TaskRecyclerView.SetAdapter(classtokcardDataAdapter);
                TaskRecyclerView.ScrollToPosition(classtokcardDataAdapter.ItemCount - loadMoreItems.Count);
            }
        }
        public void RemoveClassTokCollection(ClassTokModel classTokItem)
        {
            var collection = ClassTokCollection.FirstOrDefault(a => a.Id == classTokItem.Id);
            if (collection != null) //If item exist
            {
                int ndx = ClassTokCollection.IndexOf(collection); //Get index
                ClassTokCollection.Remove(collection);

                if (isSearchFragment)
                {
                    if (Settings.FilterToksSearch == (int)FilterToks.Toks)
                    {
                        SetClassTokRecyclerAdapter();
                    }
                    else if (Settings.FilterToksSearch == (int)FilterToks.Cards)
                    {
                        SetClassCardsRecyclerAdapter();
                    }
                }
                else
                {
                    if (Settings.FilterToksHome == (int)FilterToks.Toks)
                    {
                        SetClassTokRecyclerAdapter();
                    }
                    else if (Settings.FilterToksHome == (int)FilterToks.Cards)
                    {
                        SetClassCardsRecyclerAdapter();
                    }
                }

                if (MainActivity.Instance.tabLayout.SelectedTabPosition == 1) //Search
                {
                    //To Do: needs to delete Home toks when selected tab is Search
                    //Since home_fragment or classtoks_fragment is called from Search Fragment
                    //it will not be able to delete the toks from the root parent
                }
                
                /*if (ClassTokCollection.Count >= ndx)
                {
                    TaskRecyclerView.ScrollToPosition(ndx);
                }*/

                profile_fragment.Instance.RemoveToksCollection(classTokItem.Id);
                
                if (search_fragment.Instance.tabLayout.SelectedTabPosition == 0)
                {
                    if (search_fragment.Instance.isSearchedClicked)
                    {
                        search_fragment.Instance.fragments[0] = new classtoks_fragment(Settings.GetUserModel().UserId)
                        {
                            isSearchFragment = true,
                            filterText = search_fragment.Instance.SearchText.Text,
                            filterType = FilterType.Text,
                            filter = search_fragment.Instance.toksfilter
                        };
                        search_fragment.Instance.setupViewPager(search_fragment.Instance.viewpager, 0);
                        search_fragment.Instance.tabLayout.GetTabAt(0).Select();
                    }
                }
            }
        }
        public void OnGridBackgroundClick(object sender, int position)
        {
            int photoNum = position + 1;
            Intent nextActivity = new Intent(MainActivity.Instance, typeof(TokInfoActivity));
            classtokModel = ClassTokCollection[position];
            var modelConvert = JsonConvert.SerializeObject(classtokModel);
            nextActivity.PutExtra("classtokModel", modelConvert);
            this.StartActivityForResult(nextActivity, (int)ActivityType.HomePage);
        }
        public async Task<List<ClassTokModel>> GetClassToksData()
        {
            var taskCompletionSource = new TaskCompletionSource<List<ClassTokModel>>();
            CancellationToken cancellationToken;
            List<ClassTokModel> classTokModelsResult = new List<ClassTokModel>();
            bool isPublicFeed = false;
            if (Settings.FilterFeed == (int)FilterType.All)
            {
                isPublicFeed = true;
            }

            if (isSearchFragment)
            {
                filterBy = (FilterBy)Settings.FilterByTypeSearch;
                filterItems = JsonConvert.DeserializeObject<List<string>>(Settings.FilterByItemsSearch);
            }
            else
            {
                filterBy = (FilterBy)Settings.FilterByTypeHome;
                filterItems = JsonConvert.DeserializeObject<List<string>>(Settings.FilterByItemsHome);
            }
            
            cancellationToken.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                taskCompletionSource.TrySetCanceled();
            });

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
                cancellationToken = cancellationTokenSource.Token;

                var queryValues = new ClassTokQueryValues()
                {
                    partitionkeybase = $"{Settings.GetUserModel().UserId}-classtoks",
                    groupid = isPublicFeed ? groupId : "",
                    userid = isPublicFeed ? "" : Settings.GetUserModel().UserId,
                    text = filter,
                    startswith = false,
                    publicfeed = isPublicFeed,
                    FilterBy = filterBy,
                    FilterItems = filterItems,
                    searchvalue = isSearchFragment == true ? filterText : null
                };

                ResultData<ClassTokModel> tokResult = new ResultData<ClassTokModel>();
                tokResult.Results = new List<ClassTokModel>();
                tokResult = await ClassService.Instance.GetClassToksAsync(queryValues, cancellationToken);

                if (tokResult.ContinuationToken == "cancelled")
                {
                    ShimmerLayout.Visibility = ViewStates.Gone;
                    showRetryDialog("Task was cancelled.");
                }
                else
                {
                    classTokModelsResult = tokResult.Results.ToList();
                    TaskRecyclerView.ContentDescription = tokResult.ContinuationToken;
                }
            }

            return classTokModelsResult;
        }

        private void showRetryDialog(string message)
        {
            var builder = new Android.Support.V7.App.AlertDialog.Builder(MainActivity.Instance)
                            .SetMessage(message)
                            .SetPositiveButton("Cancel", (_, args) =>
                            {

                            })
                            .SetNegativeButton("Retry", async (_, args) =>
                            {
                                await InitializeData();
                            })
                            .SetCancelable(false)
                            .Show();
        }
        public async Task LoadMoreData()
        {
            if (!string.IsNullOrEmpty(TaskRecyclerView.ContentDescription))
            {
                var tokQueryModel = new ClassTokQueryValues();
                tokQueryModel.paginationid = TaskRecyclerView.ContentDescription;
                //tokQueryModel.loadmore = "yes";
                tokQueryModel.partitionkeybase = "classtoks";
                tokQueryModel.text = ""; // filter;
                tokQueryModel.startswith = false;
                TaskRecyclerView.ContentDescription = "";

                showBottomLoading();
                var result = await ClassService.Instance.GetClassToksAsync(tokQueryModel);
                var resultList = result.Results.ToList();
                hideBottomLoading();

                TaskRecyclerView.ContentDescription = result.ContinuationToken;
                foreach (var item in resultList)
                {
                    if (item.IsMegaTok == true || item.TokGroup.ToLower() == "mega")
                    {
                        bool isGetSection = false;
                        if (isSearchFragment)
                        {
                            if (Settings.FilterToksSearch == (int)FilterToks.Cards)
                            {
                                isGetSection = true;
                            }
                        }
                        else
                        {
                            if (Settings.FilterToksHome == (int)FilterToks.Cards)
                            {
                                isGetSection = true;
                            }
                        }

                        if (isGetSection)
                        {
                            var getTokSectionsResult = await TokService.Instance.GetTokSectionsAsync(item.Id);
                            var getTokSections = getTokSectionsResult.Results;
                            item.Sections = getTokSections.ToArray();
                        }
                    }

                    ClassTokCollection.Add(item);
                }

                if (isSearchFragment)
                {
                    if (Settings.FilterToksSearch == (int)FilterToks.Toks)
                    {
                        SetClassTokRecyclerAdapter(resultList);
                    }
                    else if (Settings.FilterToksSearch == (int)FilterToks.Cards)
                    {
                        SetClassCardsRecyclerAdapter(resultList);
                    }
                }
                else
                {
                    if (Settings.FilterToksHome == (int)FilterToks.Toks)
                    {
                        SetClassTokRecyclerAdapter(resultList);
                    }
                    else if (Settings.FilterToksHome == (int)FilterToks.Cards)
                    {
                        SetClassCardsRecyclerAdapter(resultList);
                    }
                }
            }
        }

        private void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                TextNothingFound.SetTextColor(Color.White);
                TextNothingFound.SetBackgroundColor(Color.Transparent);
                TextNothingFound.Visibility = ViewStates.Gone;

                //Data Refresh Place  
                BackgroundWorker work = new BackgroundWorker();
                work.DoWork += Work_DoWork;
                work.RunWorkerCompleted += Work_RunWorkerCompleted;
                work.RunWorkerAsync();
            }
            else
            {
                TextNothingFound.Text = "No Internet Connection!";
                TextNothingFound.SetTextColor(Color.White);
                TextNothingFound.SetBackgroundColor(Color.Black);
                TextNothingFound.Visibility = ViewStates.Visible;
                RefreshLayout.Refreshing = false;
            }
        }
        private void Work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshLayout.Refreshing = false;
        }
        private void Work_DoWork(object sender, DoWorkEventArgs e)
        {
            FilterType type = (FilterType)Settings.FilterTag;
            ((Activity)Context).RunOnUiThread(async () => await InitializeData());
            Thread.Sleep(1000);
        }
        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if ((requestCode == (int)ActivityType.HomePage) && (resultCode == -1))
            {
                var isDeleted = data.GetBooleanExtra("isDeleted", false);
                if (isDeleted)
                {
                    var tokModelstring = data.GetStringExtra("classtokModel");
                    if (tokModelstring != null)
                    {
                        classtokModel = JsonConvert.DeserializeObject<ClassTokModel>(tokModelstring);
                        RemoveClassTokCollection(classtokModel);
                    }
                }

                /*bool changesmade = data.GetBooleanExtra("changesMade", false);
                var dataClassTokModelstr = data.GetStringExtra("classtokModel");

                if (changesmade)
                {
                    if (dataClassTokModelstr != null)
                    {
                        //TokDataList
                        var dataClassTokModel = JsonConvert.DeserializeObject<ClassTokModel>(dataClassTokModelstr);
                        if (dataClassTokModel != null)
                        {
                            var result = ClassTokCollection.FirstOrDefault(c => c.Id == dataClassTokModel.Id);
                            if (result != null)
                            {
                                int ndx = ClassTokCollection.IndexOf(result);
                                ClassTokCollection.Remove(result);

                                ClassTokCollection.Insert(ndx, dataClassTokModel);
                                ClassTokDataAdapter = new ClassTokDataAdapter(ClassTokCollection, ListTokmojiModel);
                                ClassTokDataAdapter.ItemClick -= OnGridBackgroundClick;
                                ClassTokDataAdapter.ItemClick += OnGridBackgroundClick;
                                TaskRecyclerView.SetAdapter(ClassTokDataAdapter);
                                TaskRecyclerView.ScrollToPosition(ndx);
                            }
                        }
                    }
                }*/
            }
        }
        public ProgressBar progressbar => v.FindViewById<ProgressBar>(Resource.Id.progressbar);
        public RecyclerView TaskRecyclerView => v.FindViewById<RecyclerView>(Resource.Id.home_recyclerView);
        public ShimmerLayout ShimmerLayout => v.FindViewById<ShimmerLayout>(Resource.Id.home_shimmer_view_container);
        public SwipeRefreshLayout RefreshLayout => v.FindViewById<SwipeRefreshLayout>(Resource.Id.home_swiperefresh_ListToks);
        public ImageView ClassTokIconImg => v.FindViewById<ImageView>(Resource.Id.home_img_classtokicon);
        public TextView TextNothingFound => v.FindViewById<TextView>(Resource.Id.TextNothingFound);
    }
}