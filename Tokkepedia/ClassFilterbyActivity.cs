using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;
using Google.Android.Material.Tabs;
using Tokkepedia.Adapters;
using Tokkepedia.Fragments;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Xamarin.Essentials;
using XFragment = AndroidX.Fragment.App.Fragment;
using XFragmentManager = AndroidX.Fragment.App.FragmentManager;

namespace Tokkepedia
{
    [Activity(Label = "Filter By", Theme = "@style/Theme.AppCompat.Light.Dialog.NoTitle")]
    public class ClassFilterbyActivity : BaseActivity
    {
        string filterby = "", caller = "home"; FilterBy filterByEnum = FilterBy.None;
        internal static ClassFilterbyActivity Instance { get; private set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_class_filterby);

            Instance = this;
            filterby = Intent.GetStringExtra("filterby");
            caller = Intent.GetStringExtra("caller");

            var layoutManager = new LinearLayoutManager(this);
            recyclerFilterBy.SetLayoutManager(layoutManager);

            RunOnUiThread(async() => await Initialize());

            rbtnRecent.CheckedChange += delegate
            {
                RunOnUiThread(async () => await Initialize());
            };

            btnCancel.Click += delegate
            {
                this.Finish();
            };

            btnApplyFilter.Click += delegate
            {
                Intent = new Intent();
                Intent.PutExtra("filterby", (int)filterByEnum);
                Intent.PutExtra("filterByList", btnApplyFilter.ContentDescription);
                SetResult(Android.App.Result.Ok, Intent);
                Finish();
            };
        }
        private async Task Initialize()
        {
            txtProgressText.Text = "Loading...";
            linearProgress.Visibility = ViewStates.Visible;
            this.Window.AddFlags(WindowManagerFlags.NotTouchable);
            var listCommonModel = await GetMoreFilterOptions();

            int typePosition = -1;
            if (filterByEnum == FilterBy.Type)
            {
                string itemType = "";
                if (caller.ToLower() == "home")
                {
                    itemType = Settings.FilterByTypeSelectedHome;
                }
                if (caller.ToLower() == "search")
                {
                    itemType = Settings.FilterByTypeSelectedSearch;
                }
                if (caller.ToLower() == "profile")
                {
                    itemType = Settings.FilterByTypeSelectedProfile;
                }

                for (int i = 0; i < listCommonModel.Count; i++)
                {
                    if (listCommonModel[i].Title == itemType)
                    {
                        typePosition = i;
                    }
                }
            }

            var adapterClassFilter = new ClassFilterByAdapter(this, listCommonModel, (int)filterByEnum, typePosition);
            recyclerFilterBy.SetAdapter(adapterClassFilter);

            linearProgress.Visibility = ViewStates.Gone;
            this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
        }
        public async Task<List<CommonModel>> GetMoreFilterOptions()
        {
            filterByEnum = FilterBy.None;
            switch (filterby.ToLower())
            {
                case "class":
                    filterByEnum = FilterBy.Class;
                    break;
                case "category":
                    filterByEnum = FilterBy.Category;
                    break;
                case "type":
                    filterByEnum = FilterBy.Type;
                    break;
                default:
                    filterByEnum = FilterBy.None;
                    break;
            }

            List<CommonModel> listCommonModel = await ClassService.Instance.GetMoreFilterOptions(new ClassTokQueryValues()
            {
                FilterBy = filterByEnum,
                RecentOnly = rbtnRecent.Checked
            });
            return listCommonModel;
        }

        private RadioButton rbtnRecent => FindViewById<RadioButton>(Resource.Id.rbtnRecent);
        private RadioButton rbtnAZ => FindViewById<RadioButton>(Resource.Id.rbtnAZ);
        private RecyclerView recyclerFilterBy => FindViewById<RecyclerView>(Resource.Id.recyclerFilterBy);
        private Button btnCancel => FindViewById<Button>(Resource.Id.rbtnCancel);
        public Button btnApplyFilter => FindViewById<Button>(Resource.Id.rbtnApplyFilter);
        private LinearLayout linearProgress => FindViewById<LinearLayout>(Resource.Id.linearProgress);
        private TextView txtProgressText => FindViewById<TextView>(Resource.Id.txtProgressText);
    }
}