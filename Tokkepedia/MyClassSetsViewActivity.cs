using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using AndroidX.ViewPager.Widget;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using Tokkepedia.Adapters;
using Tokkepedia.Fragments;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokkepedia.Shared.ViewModels;
using Tokket.Tokkepedia;
using Xamarin.Essentials;
using XFragment = AndroidX.Fragment.App.Fragment;
using XFragmentManager = AndroidX.Fragment.App.FragmentManager;

namespace Tokkepedia
{
#if (_TOKKEPEDIA)
    [Activity(Label = "", Theme = "@style/CustomAppTheme")]
#endif

#if (_CLASSTOKS)
    [Activity(Label = "", Theme = "@style/CustomAppThemeBlue")]
#endif

    public class MyClassSetsViewActivity : BaseActivity
    {
        internal static MyClassSetsViewActivity Instance { get; private set; }
        Intent nextActivity;
        public Set SetModel; 
        ClassSetModel classSetModel; ClassSetViewModel ClassSetVM;
        string groupId = "";
        int uiOptions;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.mysetsview_page);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            Instance = this;

            /*var NavService = (NavigationService)SimpleIoc.Default.GetInstance<INavigationService>();
            var setdata = NavService.GetAndRemoveParameter<string>(Intent); //If Called from MySetsView after adding toks
            if (setdata == null)
            {
                classSetModel = JsonConvert.DeserializeObject<ClassSetModel>(Intent.GetStringExtra("classsetModel"));
                SetModel = classSetModel;

                ClassSetVM = new ClassSetViewModel();
                RunOnUiThread(async () => await GetClassToksAsync());
            }
            else
            {
                SetModel = JsonConvert.DeserializeObject<Set>(setdata);
            }*/
            var setdata = Intent.GetStringExtra("set"); //If Called from MySetsView after adding toks
            if (setdata == null)
            {
                classSetModel = JsonConvert.DeserializeObject<ClassSetModel>(Intent.GetStringExtra("classsetModel"));
                SetModel = classSetModel;
                groupId = classSetModel.GroupId;

                ClassSetVM = new ClassSetViewModel();
                RunOnUiThread(async () => await GetClassToksAsync());
            }
            else
            {
                groupId = "";
                SetModel = JsonConvert.DeserializeObject<Set>(setdata);

                setupViewPager(viewpager);
                tabLayout.SetupWithViewPager(viewpager);
            }

            imgbtnSetsTokChoice.Visibility = ViewStates.Visible;

            //Add Toks
            btnMySetsViewAdd.Tag = 0;
            btnMySetsViewAdd.Text = "Add Class Toks to Set";
            btnMySetsViewAdd.Click -= AddRemoveToks;
            btnMySetsViewAdd.Click += AddRemoveToks;

            //Remove Toks
            btnMySetsViewRemove.Tag = 1;
            btnMySetsViewRemove.Text = "Remove Class Toks";
            btnMySetsViewRemove.Click -= AddRemoveToks;
            btnMySetsViewRemove.Click += AddRemoveToks;

            imgbtnSetsTokCards.Click -= OnPlayTokCards;
            imgbtnSetsTokCards.Click += OnPlayTokCards;

            imgbtnSetsTokMatch.Click -= OnPlayTokMatch;
            imgbtnSetsTokMatch.Click += OnPlayTokMatch;

            imgbtnSetsTokChoice.Click -= OnPlayTokChoice;
            imgbtnSetsTokChoice.Click += OnPlayTokChoice;
        }
        
        private async Task GetClassToksAsync()
        {
            var classtokRes = await ClassService.Instance.GetClassToksAsync(new ClassTokQueryValues() { partitionkeybase = $"{classSetModel.Id}-classtoks", publicfeed = false });
            ClassSetVM.ClassToks = classtokRes.Results.ToList();
            ClassSetVM.ClassSet = classSetModel;

            setupViewPager(viewpager);
            tabLayout.SetupWithViewPager(viewpager);
        }
        private void AddRemoveToks(object sender, EventArgs e)
        {
            bool isAddToSet = true;
            if ((int)(sender as Button).Tag == 1)
            {
                isAddToSet = false; // set false if button clicked is remove
            }

            Settings.ActivityInt = Convert.ToInt16(ActivityType.MySetsView);
            nextActivity = new Intent(MainActivity.Instance, typeof(MyClassSetsActivity));
            nextActivity.PutExtra("isAddToSet", isAddToSet);
            nextActivity.PutExtra("TokTypeId", SetModel.TokTypeId);
            nextActivity.PutExtra("classsetModel", JsonConvert.SerializeObject(SetModel));
            this.StartActivity(nextActivity);
        }
        private void OnPlayTokCards(object sender, EventArgs e)
        {
            nextActivity = new Intent(MainActivity.Instance, typeof(TokCardsMiniGameActivity));
            nextActivity.PutExtra("setModel", JsonConvert.SerializeObject(SetModel));
            MainActivity.Instance.StartActivity(nextActivity);
        }
        private void OnPlayTokMatch(object sender, EventArgs e)
        {
            nextActivity = new Intent(MainActivity.Instance, typeof(TokMatchActivity));
            nextActivity.PutExtra("setModel", JsonConvert.SerializeObject(SetModel));
            MainActivity.Instance.StartActivity(nextActivity);
        }
        private void OnPlayTokChoice(object sender, EventArgs e)
        {
            nextActivity = new Intent(MainActivity.Instance, typeof(TokChoiceActivity));
            nextActivity.PutExtra("setModel", JsonConvert.SerializeObject(SetModel));
            MainActivity.Instance.StartActivity(nextActivity);
        }
        void setupViewPager(ViewPager viewPager)
        {
            Settings.FilterTag = (int)FilterType.Set;

            List<XFragment> fragments = new List<XFragment>();
            List<string> fragmentTitles = new List<string>();
            
            fragments.Add(new classtoks_fragment(groupId, SetModel.Id, _ClassSetVM: JsonConvert.SerializeObject(ClassSetVM))); //ClassSetVM.ClassSet.GroupId
            //fragments.Add(new myclasssets_tokcards_fragment());
            
            fragmentTitles.Add("Class Toks");
            //fragmentTitles.Add("Tok Cards");

            var adapter = new AdapterFragmentX(this.SupportFragmentManager, fragments, fragmentTitles);
            viewPager.Adapter = adapter;
            viewpager.Adapter.NotifyDataSetChanged();
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();

                    if (Settings.ActivityInt != Convert.ToInt16(ActivityType.ClassGroupActivity))
                    {
                        /*//Go back to where the Left Menu is calling from originally
                        Settings.ActivityInt = Convert.ToInt16(ActivityType.LeftMenuSets);
                        nextActivity = new Intent(this, typeof(MyClassSetsActivity));
                        nextActivity.PutExtra("isAddToSet", false);
                        nextActivity.PutExtra("TokTypeId", "");
                        nextActivity.SetFlags(ActivityFlags.ClearTop);
                        this.StartActivity(nextActivity);*/
                    }
                    
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        protected override void OnResume()
        {
            base.OnResume();
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
        }

        public override void OnBackPressed()
        {
            Console.WriteLine("");
        }
        public TextView lblMySetViewTitle => FindViewById<TextView>(Resource.Id.lblMySetViewTitle);
        public TextView lblMySetViewTokGroup => FindViewById<TextView>(Resource.Id.lblMySetViewTokGroup);
        public TextView lblMySetViewTokType => FindViewById<TextView>(Resource.Id.lblMySetViewTokType);
        public TextView lblMySetViewDescription => FindViewById<TextView>(Resource.Id.lblMySetViewDescription);
        public TextView lblMySetViewToksCnt => FindViewById<TextView>(Resource.Id.lblMySetViewToksCnt);
        public Button btnMySetsViewAdd => this.FindViewById<Button>(Resource.Id.btnMySetsViewAdd);
        public Button btnMySetsViewRemove => this.FindViewById<Button>(Resource.Id.btnMySetsViewRemove);
        public ImageButton imgbtnSetsTokCards => this.FindViewById<ImageButton>(Resource.Id.imgbtnSetsTokCards);
        public ImageButton imgbtnSetsTokMatch => this.FindViewById<ImageButton>(Resource.Id.imgbtnSetsTokMatch); 
        public ImageButton imgbtnSetsTokChoice => this.FindViewById<ImageButton>(Resource.Id.imgbtnSetsTokChoice);
        public TabLayout tabLayout => this.FindViewById<TabLayout>(Resource.Id.tabLayoutMySetsViewToks);
        public ViewPager viewpager => this.FindViewById<ViewPager>(Resource.Id.viewpagerMySetsViewToks);
    }
}