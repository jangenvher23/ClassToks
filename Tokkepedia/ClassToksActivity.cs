using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedService = Tokkepedia.Shared.Services;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using Tokket.Tokkepedia;
using Supercharge;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.RecyclerView.Widget;
using Tokkepedia.Shared.Helpers;
using Xamarin.Essentials;
using Tokkepedia.Shared.Models;
using Tokkepedia.Adapters;
using Newtonsoft.Json;
using Android.Graphics;
using Tokkepedia.Listener;
using System.ComponentModel;
using System.Threading.Tasks;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia.Tools;
using System.Threading;
using Color = Android.Graphics.Color;

namespace Tokkepedia
{
    [Activity(Label = "", Theme = "@style/CustomAppThemeBlue")]
    public class ClassToksActivity : BaseActivity
    {
        List<Tokmoji> ListTokmojiModel;
        ShimmerLayout shimmerLayout = null;
        SwipeRefreshLayout refreshLayout = null;
        GridLayoutManager mLayoutManager; RecyclerView recyclerView;
        List<ClassTokModel> TokDataList; ClassTokModel tokModel;
        ClassTokDataAdapter tokDataAdapter; ClassTokDataAdapter tokCardAdapter; FilterType type; string filter;
        internal static ClassToksActivity Instance { get; private set; }
        string strtoken;
        ISharedPreferences prefs;
        ISharedPreferencesEditor editor;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.toks_page);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            editor = prefs.Edit();

            Instance = this;

            type = (FilterType)Settings.FilterTag;
            Settings.FilterTag = (int)FilterType.All;//set back to default

            this.Title = Intent.GetStringExtra("titlepage");
            filter = Intent.GetStringExtra("filter");

            Settings.ContinuationToken = "";
            int numcol = 1;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                numcol = (int)NumDisplay.tablet + 1;
            }

            tokModel = new ClassTokModel();
            mLayoutManager = new GridLayoutManager(Application.Context, numcol);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.toks_recyclerView);
            recyclerView.SetLayoutManager(mLayoutManager);

            SupportActionBar.Title = SupportActionBar.Title; //+ " - " + Intent.GetStringExtra("headerpage").ToUpper();
            SupportActionBar.Subtitle = Intent.GetStringExtra("headerpage").ToUpper();

            UserModel myAccount = JsonConvert.DeserializeObject<UserModel>(Settings.UserAccount);
            strtoken = myAccount.StreamToken;

            this.RunOnUiThread(async () => await InitializeData());

            refreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.toks_swiperefresh_ListToks);
            refreshLayout.SetColorSchemeColors(Color.DarkViolet, Color.Violet, Color.Lavender, Color.LavenderBlush);
            refreshLayout.Refresh += RefreshLayout_Refresh;

            if (recyclerView != null)
            {
                recyclerView.HasFixedSize = true;

                var onScrollListener = new XamarinRecyclerViewOnScrollListener(mLayoutManager);
                onScrollListener.LoadMoreEvent += async (object sender, EventArgs e) =>
                {
                    if (!string.IsNullOrEmpty(Settings.ContinuationToken))
                    {
                        await GetSearchData();
                    }
                };

                recyclerView.AddOnScrollListener(onScrollListener);

                recyclerView.SetLayoutManager(mLayoutManager);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;
                case Resource.Id.btnFilter:
                    string SubTitle = SupportActionBar.Title + ": " + (SupportActionBar.Subtitle.Substring(0, 1) + SupportActionBar.Subtitle.ToLower().Substring(1, SupportActionBar.Subtitle.Length - 1));
                    Intent nextActivity = new Intent(this, typeof(FilterActivity));
                    nextActivity.PutExtra("activitycaller", "ToksActivity");
                    nextActivity.PutExtra("SubTitle", SubTitle);
                    //nextActivity.PutExtra("TokList", JsonConvert.SerializeObject(TokDataList));
                    editor.PutString("TokModelList", JsonConvert.SerializeObject(TokDataList));
                    editor.Apply();

                    StartActivityForResult(nextActivity, 20001);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        private void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            //Data Refresh Place  
            BackgroundWorker work = new BackgroundWorker();
            work.DoWork += Work_DoWork;
            work.RunWorkerCompleted += Work_RunWorkerCompleted;
            work.RunWorkerAsync();
        }
        private void Work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            refreshLayout.Refreshing = false;
        }
        private void Work_DoWork(object sender, DoWorkEventArgs e)
        {
            this.RunOnUiThread(async () => await InitializeData());
            Thread.Sleep(1000);
        }
        public async Task GetSearchData()
        {
            var tokQueryModel = new ClassTokQueryValues();
            switch (type)
            {
                case FilterType.TokType:
                    tokQueryModel.toktype = filter;
                    break;
                case FilterType.Category:
                    tokQueryModel.category = filter;
                    break;
                case FilterType.Group:
                    tokQueryModel.tokgroup = filter;
                    break;
                default:
                    break;
            }

            tokQueryModel.paginationid = Settings.ContinuationToken;
            //tokQueryModel.loadmore = "yes";
            tokQueryModel.partitionkeybase = "classtoks";
            tokQueryModel.startswith = false;
            Settings.ContinuationToken = "";
           var result = await ClassService.Instance.GetClassToksAsync(tokQueryModel);
            recyclerView.ContentDescription = result.ContinuationToken;
            Settings.ContinuationToken = result.ContinuationToken;

            //if (Settings.FilterToks == (int)FilterToks.Toks)
            //{
            tokDataAdapter.UpdateItems(result.Results.ToList(), result.Results.ToList().Count - 1);
            //}
            //else if (Settings.FilterToks == (int)FilterToks.Cards)
            //{
            //tokCardAdapter.UpdateItems(result);
            //}

            TokDataList.AddRange(result.Results.ToList());
        }
        public async Task InitializeData()
        {
            TokDataList = new List<ClassTokModel>();
            recyclerView.SetAdapter(null);
            shimmerLayout = FindViewById<ShimmerLayout>(Resource.Id.toks_shimmer_view_container);
            shimmerLayout.StartShimmerAnimation();
            shimmerLayout.Visibility = Android.Views.ViewStates.Visible;

            var result = await GetToksData();

            TokDataList = result;

            //if (Settings.FilterToks == (int)FilterToks.Toks)
            //{
            SetRecyclerAdapter(result);
            //}
            //else if (Settings.FilterToks == (int)FilterToks.Cards)
            //{
            // SetCardRecyclerAdapter(result);
            //}

            shimmerLayout.Visibility = Android.Views.ViewStates.Invisible;
        }
        private void SetRecyclerAdapter(List<ClassTokModel> tokModelRes)
        {
            tokDataAdapter = new ClassTokDataAdapter(tokModelRes, ListTokmojiModel);
            tokDataAdapter.ItemClick += OnGridBackgroundClick;
            recyclerView.SetAdapter(tokDataAdapter);
        }
        private void SetCardRecyclerAdapter(List<ClassTokModel> tokModelRes)
        {
            tokCardAdapter = new ClassTokDataAdapter(tokModelRes, ListTokmojiModel);
            recyclerView.SetAdapter(tokCardAdapter);
        }
        public async Task<List<ClassTokModel>> GetToksData()
        {
            bool isPublicFeed = false;
            if (Settings.FilterTag == (int)FilterType.All)
            {
                isPublicFeed = true;
            }

            var tokmojiResult = await TokMojiService.Instance.GetTokmojisAsync(null);
            ListTokmojiModel = tokmojiResult.Results.ToList();

            List<ClassTokModel> classTokModelsResult = new List<ClassTokModel>();
            ResultData<ClassTokModel> tokResult = new ResultData<ClassTokModel>();
            tokResult.Results = new List<ClassTokModel>();
            switch (type)
            {
                case FilterType.TokType:
                    tokResult = await ClassService.Instance.GetClassToksAsync(new ClassTokQueryValues()
                    {
                        partitionkeybase = $"{Settings.GetUserModel().UserId}-classtoks",
                        groupid = "",
                        userid = Settings.GetUserModel().UserId,
                        toktype = filter,
                        text = filter,
                        startswith = false,
                        publicfeed = isPublicFeed,
                        FilterBy = FilterBy.Type,
                        FilterItems = new List<string>(),
                        searchvalue = null
                    });
                    break;
                case FilterType.Category:
                    tokResult = await ClassService.Instance.GetClassToksAsync(new ClassTokQueryValues()
                    {
                        partitionkeybase = $"{Settings.GetUserModel().UserId}-classtoks",
                        groupid = "",
                        userid = Settings.GetUserModel().UserId,
                        category = filter,
                        startswith = false,
                        publicfeed = isPublicFeed,
                        FilterBy = FilterBy.Category,
                        FilterItems = new List<string>(),
                        searchvalue = null
                    });
                    break;
                case FilterType.Group:
                    tokResult = await ClassService.Instance.GetClassToksAsync(new ClassTokQueryValues()
                    {
                        partitionkeybase = $"{Settings.GetUserModel().UserId}-classtoks",
                        groupid = "",
                        userid = Settings.GetUserModel().UserId,
                        tokgroup = filter,
                        startswith = false,
                        publicfeed = isPublicFeed,
                        FilterBy = FilterBy.None,
                        FilterItems = new List<string>(),
                        searchvalue = null
                    });
                    break;
                default:
                    tokResult = await ClassService.Instance.GetClassToksAsync(new ClassTokQueryValues()
                    {
                        partitionkeybase = $"{Settings.GetUserModel().UserId}-classtoks",
                        groupid = "",
                        userid = Settings.GetUserModel().UserId,
                        startswith = false,
                        publicfeed = isPublicFeed,
                        FilterBy = FilterBy.None,
                        FilterItems = new List<string>(),
                        searchvalue = null
                    });
                    break;
            }

            return tokResult.Results.ToList();
        }
        public void OnGridBackgroundClick(object sender, int position)
        {
            int photoNum = position + 1;
            Intent nextActivity = new Intent(MainActivity.Instance, typeof(TokInfoActivity));
            tokModel = TokDataList[position];
            var modelConvert = JsonConvert.SerializeObject(tokModel);
            nextActivity.PutExtra("tokModel", modelConvert);
            this.StartActivityForResult(nextActivity, 10001);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if ((requestCode == 10001) && (resultCode == Android.App.Result.Ok))
            {
                var isDeleted = data.GetBooleanExtra("isDeleted", false);
                if (isDeleted)
                {
                    TokDataList.Remove(tokModel);
                    SetRecyclerAdapter(TokDataList);
                }
            }
            else if ((requestCode == (int)ActivityType.AddStickerDialogActivity) && (resultCode == Android.App.Result.Ok))
            {
                var dataTokModelstr = data.GetStringExtra("classtokModel");
                if (dataTokModelstr != null)
                {
                    var dataTokModel = JsonConvert.DeserializeObject<ClassTokModel>(dataTokModelstr);
                    if (dataTokModel != null)
                    {
                        var result = TokDataList.FirstOrDefault(c => c.Id == dataTokModel.Id);
                        if (result != null) //If Edit
                        {
                            int ndx = TokDataList.IndexOf(result);
                            TokDataList.Remove(result);

                            TokDataList.Insert(ndx, dataTokModel);
                            SetRecyclerAdapter(TokDataList);
                        }
                    }
                }
            }
            else if (requestCode == 20001 && resultCode == Android.App.Result.Ok) //From FilterActivity
            {
                this.RunOnUiThread(async () => await InitializeData());
            }
        }
    }
}