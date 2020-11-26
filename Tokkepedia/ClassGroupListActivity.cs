using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using GalaSoft.MvvmLight.Helpers;
using Newtonsoft.Json;
using Tokkepedia.Listener;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia.Tools;
using Xamarin.Essentials;

namespace Tokkepedia
{
    [Activity(Label = "Class Groups", Theme = "@style/CustomAppThemeBlue")]
    public class ClassGroupListActivity : BaseActivity
    {
        internal static ClassGroupListActivity Instance { get; private set; }
        ObservableCollection<ClassGroupModel> ClassGroupCollection;
        GridLayoutManager mLayoutManager;
        private enum ActivityName
        {
            Filter = 1001
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.classgroupslist_page);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            Settings.ActivityInt = (int)ActivityType.ClassGroupListActivity;

            Instance = this;

            int numcol = 1;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                numcol = (int)NumDisplay.tablet + 1;
            }
            mLayoutManager = new GridLayoutManager(Application.Context, numcol);
            RecyclerClassGroupList.SetLayoutManager(mLayoutManager);

            ClassGroupCollection = new ObservableCollection<ClassGroupModel>();

            RunOnUiThread(async() => await Initialize());

            AddClassGroupButton.Click += delegate
            {
                var nextActivity = new Intent(this, typeof(AddClassGroupActivity));
                this.StartActivity(nextActivity);
            };

            swipeRefreshRecycler.SetColorSchemeColors(Color.DarkViolet, Color.Violet, Color.Lavender, Color.LavenderBlush);
            swipeRefreshRecycler.Refresh += RefreshLayout_Refresh;

            if (RecyclerClassGroupList != null)
            {
                RecyclerClassGroupList.HasFixedSize = true;

                var onScrollListener = new XamarinRecyclerViewOnScrollListenerV7(mLayoutManager);
                onScrollListener.LoadMoreEvent += async (object sender, EventArgs e) =>
                {
                    if (!string.IsNullOrEmpty(RecyclerClassGroupList.ContentDescription))
                    {
                        await Initialize(RecyclerClassGroupList.ContentDescription);
                    }
                };

                RecyclerClassGroupList.AddOnScrollListener(onScrollListener);

                RecyclerClassGroupList.SetLayoutManager(mLayoutManager);
            }
        }
        private void showBottomDialog()
        {
            bottomProgress.Visibility = ViewStates.Visible;
        }
        private void hideBottomDialog()
        {
            bottomProgress.Visibility = ViewStates.Gone;
        }
        private async Task Initialize(string pagination_id = "")
        {
            GroupFilter filter = (GroupFilter)Settings.FilterGroup;
            ResultData<ClassGroupModel> results = new ResultData<ClassGroupModel>();

            int lastposition = 0;
            if (!string.IsNullOrEmpty(pagination_id))
            {
                lastposition = RecyclerClassGroupList.ChildCount - 1;
                showBottomDialog();
            }
            switch (filter)
            {
                case GroupFilter.OwnGroup:
                    results = await ClassService.Instance.GetClassGroupAsync(new ClassGroupQueryValues()
                    {
                        paginationid = pagination_id,
                        partitionkeybase = "classgroups",
                        userid = Settings.GetTokketUser().Id,
                        startswith = false,
                        joined = false
                    });
                    break;
                case GroupFilter.JoinedGroup:
                    results = await ClassService.Instance.GetClassGroupAsync(new ClassGroupQueryValues()
                    {
                        paginationid = pagination_id,
                        partitionkeybase = "classgroups",
                        userid = Settings.GetTokketUser().Id,
                        joined = true,
                        startswith = false
                    });
                    break;
                case GroupFilter.MyGroup:
                    var myGroups = await ClassService.Instance.GetClassGroupAsync(new ClassGroupQueryValues()
                    {
                        paginationid = pagination_id,
                        partitionkeybase = "classgroups",
                        userid = Settings.GetTokketUser().Id,
                        joined = false,
                        startswith = false
                    });

                    var joined = await ClassService.Instance.GetClassGroupAsync(new ClassGroupQueryValues()
                    {
                        paginationid = pagination_id,
                        partitionkeybase = "classgroups",
                        userid = Settings.GetTokketUser().Id,
                        joined = true,
                        startswith = false
                    });

                    var combined = myGroups.Results.ToList();
                    combined.AddRange(joined.Results);

                    results.Results = combined;

                    break;           
            }
            if (!string.IsNullOrEmpty(pagination_id))
            {
                hideBottomDialog();
            }

            RecyclerClassGroupList.ContentDescription = results.ContinuationToken;
            var classgroupResult = results.Results.ToList();

            foreach (var item in classgroupResult)
            {
                ClassGroupCollection.Add(item);
            }

            SetRecyclerAdapter();

            if (!string.IsNullOrEmpty(pagination_id))
            {
                RecyclerClassGroupList.ScrollToPosition(lastposition);
            }
        }
        public void AddClassGroupCollection(ClassGroupModel item, bool isSave = true)
        {
            if (isSave == false)
            {
                var result = ClassGroupCollection.FirstOrDefault(c => c.Id == item.Id);
                if (result != null) //If Edit
                {
                    int ndx = ClassGroupCollection.IndexOf(result);
                    ClassGroupCollection.Remove(result);

                    ClassGroupCollection.Insert(ndx, item);
                }
            }
            else
            {
                ClassGroupCollection.Insert(0, item);
            }
            SetRecyclerAdapter();
        }
        public void RemoveClassGroupCollection(ClassGroupModel item)
        {
            var collection = ClassGroupCollection.FirstOrDefault(a => a.Id == item.Id);
            if (collection != null) //If item exist
            {
                int ndx = ClassGroupCollection.IndexOf(collection); //Get index
                ClassGroupCollection.Remove(collection);

                SetRecyclerAdapter();
            }
        }
        private void SetRecyclerAdapter()
        {
            var adapterClassGroup = ClassGroupCollection.GetRecyclerAdapter(BindClassGroupViewHolder, Resource.Layout.classgrouplist_row);
            RecyclerClassGroupList.SetAdapter(adapterClassGroup);

            linearProgress.Visibility = ViewStates.Invisible;
        }
        private void BindClassGroupViewHolder(CachingViewHolder holder, ClassGroupModel model, int position)
        {
            var ClassGroupHeader = holder.FindCachedViewById<TextView>(Resource.Id.TextClassGroupHeader);
            var ClassGroupBody = holder.FindCachedViewById<TextView>(Resource.Id.TextClassGroupBody);
            var ClassGroupFooter = holder.FindCachedViewById<TextView>(Resource.Id.TextClassGroupFooter);

            var LinearClassGroupNoImage = holder.FindCachedViewById<LinearLayout>(Resource.Id.LinearClassGroupNoImage);
            var LinearClassGroupImage = holder.FindCachedViewById<LinearLayout>(Resource.Id.LinearClassGroupImage);

            var TextClassGroupHeaderImg = holder.FindCachedViewById<TextView>(Resource.Id.TextClassGroupHeaderImg);
            var imgClassGroupListImg = holder.FindCachedViewById<ImageView>(Resource.Id.imgClassGroupListImg);
            var TextClassGroupBodyImg = holder.FindCachedViewById<TextView>(Resource.Id.TextClassGroupBodyImg);

            LinearClassGroupNoImage.Tag = position;
            LinearClassGroupImage.Tag = position;

            if (!string.IsNullOrEmpty(model.Image))
            {
                LinearClassGroupNoImage.Visibility = ViewStates.Gone;
                LinearClassGroupImage.Visibility = ViewStates.Visible;
                TextClassGroupHeaderImg.Text = model.Name;
                TextClassGroupBodyImg.Text = model.Description;
                Glide.With(this).Load(model.Image).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).Error(Resource.Drawable.no_image).FitCenter()).Into(imgClassGroupListImg);
            }
            else
            {
                LinearClassGroupNoImage.Visibility = ViewStates.Visible;
                LinearClassGroupImage.Visibility = ViewStates.Gone;
                ClassGroupHeader.Text = model.Name;
                ClassGroupBody.Text = model.Description;
                ClassGroupFooter.Text = "Last updated " + DateConvert.ConvertToRelative(model.CreatedTime);
            }
        }
        [Java.Interop.Export("ItemRowClicked")]
        public void ItemRowClicked(View v)
        {
            int position = 0;
            try { position = (int)v.Tag; } catch { position = int.Parse((string)v.Tag); }
           // var resultGroupItem = await ClassService.Instance.GetClassGroupAsync(ClassGroupCollection[position].Id);
            Intent nextActivity = new Intent(this, typeof(ClassGroupActivity));
            var modelConvert = JsonConvert.SerializeObject(ClassGroupCollection[position]);
            nextActivity.PutExtra("ClassGroupModel", modelConvert);
            this.StartActivity(nextActivity);
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
                    var nextActivity = new Intent(this, typeof(FilterActivity));
                    nextActivity.PutExtra("activitycaller", "Home");
                    StartActivityForResult(nextActivity, (int)ActivityName.Filter);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == (int)ActivityName.Filter && resultCode == Android.App.Result.Ok) //Filter
            {
                //Result from Filter
            }
        }

        private void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                //Data Refresh Place  
                BackgroundWorker work = new BackgroundWorker();
                work.DoWork += Work_DoWork;
                work.RunWorkerCompleted += Work_RunWorkerCompleted;
                work.RunWorkerAsync();
            }
            else
            {
                swipeRefreshRecycler.Refreshing = false;
            }
        }
        private void Work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            swipeRefreshRecycler.Refreshing = false;
        }
        private void Work_DoWork(object sender, DoWorkEventArgs e)
        {
            FilterType type = (FilterType)Settings.FilterTag;
            this.RunOnUiThread(async () => await Initialize());
            Thread.Sleep(1000);
        }

        public LinearLayout linearProgress => FindViewById<LinearLayout>(Resource.Id.linearProgress);
        public ProgressBar bottomProgress => FindViewById<ProgressBar>(Resource.Id.bottomProgress);
        public FloatingActionButton AddClassGroupButton => FindViewById<FloatingActionButton>(Resource.Id.fab_AddClassGroup);
        public RecyclerView RecyclerClassGroupList => FindViewById<RecyclerView>(Resource.Id.RecyclerClassGroupList);
        public ProgressBar progressbar => FindViewById<ProgressBar>(Resource.Id.progressbar);
        public TextView progressBarinsideText => FindViewById<TextView>(Resource.Id.progressBarinsideText);
        public SwipeRefreshLayout swipeRefreshRecycler => FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshRecycler);
    }
}