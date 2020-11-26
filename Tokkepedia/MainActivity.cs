using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Preference;
using AndroidX.ViewPager.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Google.Android.Material.AppBar;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Tabs;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Plugin.Permissions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tokkepedia.Adapters;
using Tokkepedia.Fragments;
using Tokkepedia.Helpers;
using Tokkepedia.Shared.Extensions;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.ViewModels;
using Tokket.Tokkepedia;
using Xamarin.Essentials;
using ServiceAccount = Tokkepedia.Shared.Services;
using XFragment = AndroidX.Fragment.App.Fragment;

namespace Tokkepedia
{
    [Activity(Label = "Main Activity", Theme = "@style/AppTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : BaseActivity, View.IOnTouchListener
    {
        private bool bulb_outlined = true;
        internal static MainActivity Instance { get; private set; }
        public DrawerLayout drawerLayout;
        NavigationView m_navigationView;
        private ActionBarDrawerToggle mDrawerToggle;
        AdapterStateFragmentX adapter;
        //public Android.Support.V4.View.ViewPager mainviewpager;
        public Tokkepedia.LockableViewPager MainViewPager;
        List<XFragment> fragments = new List<XFragment>();
        List<string> fragmentTitles = new List<string>();
        public TabLayout tabLayout; Intent nextActivity;
        public ProfilePageViewModel ProfileVm => App.Locator.ProfilePageVM;
        public HomePageViewModel HomeVm => App.Locator.HomePageVM;
        ISharedPreferences prefs;
        ISharedPreferencesEditor editor;
        Android.Graphics.Color defaultPrimaryColor, defaultPrimaryDarkerColor;
        private enum ActivityName
        {
            ActivityHome = 500,
            ActivityProfile = 501
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (Settings.CurrentTheme == 1)
            {
                this.SetTheme(Android.Resource.Style.ThemeBlackNoTitleBar);
            }

            base.OnCreate(savedInstanceState);

            Platform.Init(this, savedInstanceState);

            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            editor = prefs.Edit();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

#if (_CLASSTOKS)
            imageViewLogoText.SetImageResource(Resource.Drawable.classtok_text);
#endif

            Instance = this;

            AppCenter.Start("f55f6052-d41d-4625-9dac-56bbeec98599",
                   typeof(Analytics), typeof(Crashes));

            Settings.ImageBrowseCrop = null;
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawerLayout);
            m_navigationView = FindViewById<NavigationView>(Resource.Id.main_navigation_view);

            //setting the header layout values
            var headerView = m_navigationView.GetHeaderView(0);
            var userphoto = headerView.FindViewById<ImageView>(Resource.Id.img_profileUserPhoto);
            var user = headerView.FindViewById<TextView>(Resource.Id.lblUser);
            var groupSelection = headerView.FindViewById<ImageView>(Resource.Id.lblGroupSelect);
            Glide.With(this).Load(Settings.GetTokketUser().UserPhoto).Into(userphoto);

            var displayName = Settings.GetTokketUser().DisplayName;
            if (displayName.Length > 25)
            {
                user.Text = displayName.Substring(0, 25) + "...";
            }
            else
            {
                user.Text = displayName;
            }

            if (Settings.GetTokketUser().AccountType == "group")
            {
                var subaccount = Settings.GetTokketSubaccount();
                if (subaccount?.SubaccountName.Length > 25)
                {
                    txtGroup.Text = subaccount.SubaccountName.Substring(0, 25) + "...";
                }
                else
                {
                    txtGroup.Text = subaccount.SubaccountName;
                }
            }

            if (Settings.GetTokketUser().AccountType == "individual")
            {
                txtGroup.Text = Settings.GetTokketUser().TitleId;

                //If user have title/s
                if (!string.IsNullOrEmpty(txtGroup.Text))
                {
                    groupSelection.Visibility = ViewStates.Visible;
                }
                else
                {
                    groupSelection.Visibility = ViewStates.Gone;
                }
            }

            //start coins animation
            var GifCoinIcon = headerView.FindViewById<ImageView>(Resource.Id.gif_profileCoins);
            Glide.With(this)
                .Load(Resource.Drawable.gold)
                .Into(GifCoinIcon);

            /*Stream input = Resources.OpenRawResource(Resource.Drawable.gold);
            byte[] bytes = ConvertByteArray(input);            
            GifCoinIcon.SetBytes(bytes);
            GifCoinIcon.StartAnimation();
            GifCoinIcon.SetOnTouchListener(this);*/

            //setting the value of coins
            long longcoins = Settings.UserCoins;

            TextView TextProfileCoins = headerView.FindViewById<TextView>(Resource.Id.TextProfileCoins);
            TextProfileCoins.Text = longcoins.ToKMB();

            //setting the value of the quote
            GetQuoteOfTheHour(m_navigationView);

            //mainviewpager = FindViewById<Android.Support.V4.View.ViewPager>(Resource.Id.viewpager);
            MainViewPager = FindViewById<LockableViewPager>(Resource.Id.viewpager);
            //MainViewPager = new LockableViewPager(this);
            MainViewPager.SwipeLocked = true;

#if (_TOKKEPEDIA)
            MainViewPager.OffscreenPageLimit = 4; //This is used in order to avoid the pages to keep on refreshing
#endif
#if (_CLASSTOKS)
            MainViewPager.OffscreenPageLimit = 3;
#endif

            loadHomeFragments();
            setupAppBar();

            if (mDrawerToggle == null)
            {
                setupDrawerLayout();
            }

            m_navigationView.NavigationItemSelected -= M_navigationView_NavigationItemSelected;
            m_navigationView.NavigationItemSelected += M_navigationView_NavigationItemSelected;

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            //SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_home24_dp);
            setupViewPager(MainViewPager);
            tabLayout = this.FindViewById<TabLayout>(Resource.Id.tabLayout);
            tabLayout.SetupWithViewPager(MainViewPager);
            setupHomeTabIcons();

#if (_CLASSTOKS)
            ScrollClassToks.Visibility = ViewStates.Visible;
            ScrollNormalToks.Visibility = ViewStates.Gone;
#endif

            fab_AddTok.Click += (object sender, EventArgs e) =>
            {
                if (fragmentTitles[0].ToLower() == "home")
                {
                    nextActivity = new Intent(this, typeof(AddTokActivity));
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    this.StartActivity(nextActivity);
                }
                else
                {
                    nextActivity = new Intent(this, typeof(AddClassTokActivity));
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    this.StartActivity(nextActivity);
                }
            };

            MainViewPager.PageSelected -= Viewpager_PageSelected;
            MainViewPager.PageSelected += Viewpager_PageSelected;
            setMainColors();
        }
        private void setMainColors()
        {
            var headerlayout = m_navigationView.GetHeaderView(0);
#if (_TOKKEPEDIA)
            var tokgroup_b = (ImageView)FindViewById(Resource.Id.tokgroup_b);
            var play_icon = (ImageView)FindViewById(Resource.Id.play_icon);
            var class_b = (ImageView)FindViewById(Resource.Id.class_b);
            var settings_black_36dp = (ImageView)FindViewById(Resource.Id.settings_black_36dp);
            var tokgroup_bQuote = (ImageView)FindViewById(Resource.Id.tokgroup_bQuote);
            // Black is Settings.CurrentTheme = 0          
            // White is Settings.CurrentTheme = 1
            tokgroup_b.SetColorFilter(Settings.CurrentTheme == 0 ?
                Android.Graphics.Color.ParseColor("#000000") :
                Android.Graphics.Color.ParseColor("#ffffff"));
            play_icon.SetColorFilter(Settings.CurrentTheme == 0 ?
                Android.Graphics.Color.ParseColor("#000000") :
                Android.Graphics.Color.ParseColor("#ffffff"));
            class_b.SetColorFilter(Settings.CurrentTheme == 0 ?
                Android.Graphics.Color.ParseColor("#000000") :
                Android.Graphics.Color.ParseColor("#ffffff"));
            settings_black_36dp.SetColorFilter(Settings.CurrentTheme == 0 ?
                Android.Graphics.Color.ParseColor("#000000") :
                Android.Graphics.Color.ParseColor("#ffffff"));
            tokgroup_bQuote.SetColorFilter(Settings.CurrentTheme == 0 ?
                Android.Graphics.Color.ParseColor("#000000") :
                Android.Graphics.Color.ParseColor("#ffffff"));
#endif

#if (_CLASSTOKS)
            var imageViewClassSets = (ImageView)FindViewById(Resource.Id.imageViewClassSets);
            var imageViewClassGroup = (ImageView)FindViewById(Resource.Id.imageViewClassGroup);
            var imageViewClassNotification = (ImageView)FindViewById(Resource.Id.imageViewClassNotification);
            var imageViewClassGuide = (ImageView)FindViewById(Resource.Id.imageViewClassGuide);
            var imageViewClassFolder = (ImageView)FindViewById(Resource.Id.imageViewClassFolder);
            var imageViewClassSettings = (ImageView)FindViewById(Resource.Id.imageViewClassSettings);
            // Black is Settings.CurrentTheme = 0          
            // White is Settings.CurrentTheme = 1
            imageViewClassSets.SetColorFilter(Settings.CurrentTheme == 0 ?
                Android.Graphics.Color.ParseColor("#000000") :
                Android.Graphics.Color.ParseColor("#ffffff"));
            imageViewClassGroup.SetColorFilter(Settings.CurrentTheme == 0 ?
                Android.Graphics.Color.ParseColor("#000000") :
                Android.Graphics.Color.ParseColor("#ffffff"));
            imageViewClassNotification.SetColorFilter(Settings.CurrentTheme == 0 ?
                Android.Graphics.Color.ParseColor("#000000") :
                Android.Graphics.Color.ParseColor("#ffffff"));
            imageViewClassGuide.SetColorFilter(Settings.CurrentTheme == 0 ?
                Android.Graphics.Color.ParseColor("#000000") :
                Android.Graphics.Color.ParseColor("#ffffff"));
            imageViewClassFolder.SetColorFilter(Settings.CurrentTheme == 0 ?
                Android.Graphics.Color.ParseColor("#000000") :
                Android.Graphics.Color.ParseColor("#ffffff"));
            imageViewClassSettings.SetColorFilter(Settings.CurrentTheme == 0 ?
                Android.Graphics.Color.ParseColor("#000000") :
                Android.Graphics.Color.ParseColor("#ffffff"));
#endif

#if (_TOKKEPEDIA)
            defaultPrimaryColor = new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.primary));
            defaultPrimaryDarkerColor = new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.primary_darker));
            fab_AddTok.BackgroundTintList = ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#8a00d9"));
#endif

#if (_CLASSTOKS)
            defaultPrimaryColor = new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent));
            defaultPrimaryDarkerColor = new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent_darker));
#endif

            headerlayout.FindViewById(Resource.Id.relativeLayoutHeader).SetBackgroundColor(Settings.CurrentTheme == 0 ?
                defaultPrimaryColor :
                defaultPrimaryDarkerColor);

            MainToolbar.SetBackgroundColor(Settings.CurrentTheme == 0 ?
                defaultPrimaryColor :
                defaultPrimaryDarkerColor);

            tabLayout.SetBackgroundColor(Settings.CurrentTheme == 0 ?
                defaultPrimaryColor :
                defaultPrimaryDarkerColor);

            BulbImg.SetImageResource(Settings.CurrentTheme == 0 ?
                Resource.Drawable.bulb_outlined :
                Resource.Drawable.bulb_filled);
            bulb_outlined = Settings.CurrentTheme == 0 ? true : false;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            if (e.Action == MotionEventActions.Up)
            {
                if (v.ContentDescription == "coins")
                {
                    LinearCoinsToast.Visibility = ViewStates.Gone;
                }
            }
            else if (e.Action == MotionEventActions.Down)
            {
                if (v.ContentDescription == "coins")
                {
                    LinearCoinsToast.Visibility = ViewStates.Visible;
                    TextCoinsToast.Text = string.Format("{0:#,##0.##}", Settings.UserCoins);
                }
            }
            return true;
        }

        /*private byte[] ConvertByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);
                return ms.ToArray();
            }
        }*/

        private void loadHomeFragments()
        {
            fragments.Clear();
            fragmentTitles.Clear();
#if (_TOKKEPEDIA)
            fragments.Add(new home_fragment());
            fragmentTitles.Add("Home");
#endif
#if (_CLASSTOKS)
            fragments.Add(new classtoks_fragment(Settings.GetUserModel().UserId));
            fragmentTitles.Add("Class Toks");
#endif
            fragments.Add(new search_fragment());

#if (_TOKKEPEDIA)
            fragments.Add(new notification_fragment());
#endif
            fragments.Add(new profile_fragment());
            
            fragmentTitles.Add("Search");
#if (_TOKKEPEDIA)
            fragmentTitles.Add("Notifications");
#endif
            fragmentTitles.Add("Profile");
        }
        private void setupHomeTabIcons()
        {
#if (_TOKKEPEDIA)
            tabLayout.GetTabAt(0).SetIcon(Resource.Drawable.ic_home48_dp);
            tabLayout.GetTabAt(1).SetIcon(Resource.Drawable.ic_search_white_24dp);
            tabLayout.GetTabAt(2).SetIcon(Resource.Drawable.ic_notification_white48);
            tabLayout.GetTabAt(3).SetIcon(Resource.Drawable.ic_profile_white48);
#endif
#if (_CLASSTOKS)
            setupClassTokTabIcons();
#endif
        }
        private void setupClassTokTabIcons()
        {
            tabLayout.GetTabAt(0).SetIcon(Resource.Drawable.classtok_white_48);
            tabLayout.GetTabAt(1).SetIcon(Resource.Drawable.ic_search_white_24dp);
            //tabLayout.GetTabAt(2).SetIcon(Resource.Drawable.ic_notification_white48);
            tabLayout.GetTabAt(2).SetIcon(Resource.Drawable.ic_profile_white48);
        }
        void setupViewPager(ViewPager viewPager)
        {
            adapter = new AdapterStateFragmentX(SupportFragmentManager, fragments, fragmentTitles);
            viewPager.Adapter = adapter;
            MainViewPager.Adapter.NotifyDataSetChanged();
            adapter.NotifyDataSetChanged();
            //mainviewpager.SetPageTransformer(true, new FadeTransformerViewPager());
        }
        private void setupAppBar()
        {
            //toolbar = this.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.appbar);
            SetSupportActionBar(MainToolbar);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            menu.Clear();

#if (_TOKKEPEDIA)
             if (tabLayout.SelectedTabPosition == 3) //Profile Tab
#endif
#if (_CLASSTOKS)
            if (tabLayout.SelectedTabPosition == 2) //Profile Tab
#endif
            {
                MenuInflater.Inflate(Resource.Menu.profile_menu, menu);

                //IMenuItem Android.Resource.Id
                var itemReport = menu.FindItem(Resource.Id.item_Report);

                itemReport.SetVisible(false);
            }
#if (_TOKKEPEDIA)
            else if (tabLayout.SelectedTabPosition == 2) //notification
            {

            }
#endif
            else
            {
                MenuInflater.Inflate(Resource.Menu.main, menu);
            }
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            menu.Clear();
#if (_TOKKEPEDIA)
            if (tabLayout.SelectedTabPosition == 3) //Profile
#endif
#if (_CLASSTOKS)
            if (tabLayout.SelectedTabPosition == 2) //Profile
#endif
            {
                MenuInflater.Inflate(Resource.Menu.profile_menu, menu);

                //IMenuItem Android.Resource.Id
                var itemReport = menu.FindItem(Resource.Id.item_Report);

                itemReport.SetVisible(false);
            }
#if (_TOKKEPEDIA)
            else if (tabLayout.SelectedTabPosition == 2) //Notification
            {

            }
#endif
            else
            {
                MenuInflater.Inflate(Resource.Menu.main, menu);
            }

            return base.OnPrepareOptionsMenu(menu);
        }
        public override void OnBackPressed()
        {
            Settings.ActivityInt = (int)ActivityType.HomePage;
            base.OnBackPressed();
        }
        private void setupDrawerLayout()
        {
            drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawerLayout);
            mDrawerToggle = new ActionBarDrawerToggle(this, drawerLayout, MainToolbar, Resource.String.opened, Resource.String.closed);
            drawerLayout.AddDrawerListener(mDrawerToggle);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);

            /* This little thing will display our actual hamburger icon*/
            if (mDrawerToggle == null)
            {
                setupDrawerLayout();
            }
            mDrawerToggle.SyncState();
        }
        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            mDrawerToggle.OnConfigurationChanged(newConfig);
        }
        //When This is Clicked from the profileTab
        [Java.Interop.Export("OnClickProfileImage")]
        public void OnClickProfileImage(View v)
        {
            if (ProfileVm.tokketUser.Id == Settings.GetUserModel().UserId)
            {
                Settings.BrowsedImgTag = Convert.ToInt32(v.ContentDescription);
                bottomsheet_userphoto_fragment bottomsheet = new bottomsheet_userphoto_fragment(this, profile_fragment.Instance.ProfileUserPhoto);
                bottomsheet.Show(this.SupportFragmentManager, "tag");
                /*Intent myIntent = new Intent();
                myIntent.SetType("image/*");
                myIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(myIntent, "Select Picture"), (int)ActivityType.ProfileTabActivity);*/
            }
        }

        [Java.Interop.Export("OnClickCoverPhoto")]
        public void OnClickCoverPhoto(View v)
        {
            if (ProfileVm.tokketUser.Id == Settings.GetUserModel().UserId)
            {
                Settings.BrowsedImgTag = Convert.ToInt32(v.ContentDescription);
                bottomsheet_userphoto_fragment bottomsheet = new bottomsheet_userphoto_fragment(this, profile_fragment.Instance.ProfileCoverPhoto);
                bottomsheet.Show(this.SupportFragmentManager, "tag");

                /*Intent = new Intent();
                Intent.SetType("image/*");
                Intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), (int)ActivityType.ProfileActivity);*/
            }
        }

        [Java.Interop.Export("OnClickProfileUserImage")]
        public void OnClickProfileUserImage(View v)
        {
            float currentPosition = profile_fragment.Instance.ParentImageViewer.TranslationY;
            if (currentPosition != profile_fragment.Instance.ViewDummyForTouch.TranslationY)
            {
                profile_fragment.Instance.ParentImageViewer.TranslationY = profile_fragment.Instance.ViewDummyForTouch.TranslationY;
            }

            this.tabLayout.Visibility = ViewStates.Gone;
            this.SupportActionBar.Hide();
            this.fab_AddTok.Visibility = ViewStates.Gone;

            profile_fragment.Instance.ParentImageViewer.Visibility = ViewStates.Visible;
            Animation myAnim = AnimationUtils.LoadAnimation(this, Resource.Animation.fab_scaleup);
            profile_fragment.Instance.ParentImageViewer.StartAnimation(myAnim);

            if (v.ContentDescription == "0")
            {
                //profile_fragment.Instance.ImgUserImageView.SetBackgroundColor(ManipulateColor.manipulateColor(ProfileVm.GListenerUserPhoto.mColorPalette, 0.62f));
                profile_fragment.Instance.ImgUserImageView.SetImageDrawable(profile_fragment.Instance.ProfileUserPhoto.Drawable);
                profile_fragment.Instance.ImgUserImageView.SetBackgroundColor(GetDominantColor.GetDominantColorImg(((BitmapDrawable)profile_fragment.Instance.ProfileUserPhoto.Drawable).Bitmap, 0.62f));
            }
            else if (v.ContentDescription == "1")
            {
                //profile_fragment.Instance.ImgUserImageView.SetBackgroundColor(ManipulateColor.manipulateColor(ProfileVm.GListenerCover.mColorPalette, 0.62f));
                profile_fragment.Instance.ImgUserImageView.SetImageDrawable(profile_fragment.Instance.ProfileCoverPhoto.Drawable);
                profile_fragment.Instance.ImgUserImageView.SetBackgroundColor(GetDominantColor.GetDominantColorImg(((BitmapDrawable)profile_fragment.Instance.ProfileCoverPhoto.Drawable).Bitmap, 0.62f));
            }

            profile_fragment.Instance.imgScale = profile_fragment.Instance.ImgUserImageView.Scale;
            RequestedOrientation = ScreenOrientation.Unspecified;
        }
        [Java.Interop.Export("OnClickCloseImgView")]
        public void OnClickCloseImgView(View v)
        {
            profile_fragment.Instance.ParentImageViewer.Visibility = ViewStates.Gone;
            profile_fragment.Instance.SwipeRefreshProfile.Enabled = true;
            this.tabLayout.Visibility = ViewStates.Visible;
            this.SupportActionBar.Show();
            this.fab_AddTok.Visibility = ViewStates.Visible;
            RequestedOrientation = ScreenOrientation.Portrait;
        }

        [Java.Interop.Export("onClickGroupSelect")]
        public void onClickGroupSelect(View v)
        {
            if (Settings.GetTokketUser().AccountType == "individual")
            {
                Intent nextActivity = new Intent(this, typeof(ProfileTitleActivity));
                StartActivity(nextActivity);
            }
            else
            {
                Intent nextActivity = new Intent(this, typeof(SubAccountActivity));
                StartActivity(nextActivity);
            }
        }


        [Java.Interop.Export("onClickBulb")]
        public void onClickBulb(View v)
        {
            if (bulb_outlined)
            {
                //Dark
                bulb_outlined = false;
                Settings.CurrentTheme = 1;
            }
            else
            {
                bulb_outlined = true;
                Settings.CurrentTheme = 0;
            }
            Intent nextActivity = new Intent(this, typeof(MainActivity));
            Finish();
            this.OverridePendingTransition(0, 0);
            StartActivity(nextActivity);
        }

        [Java.Interop.Export("onRedirectProfile")]
        public void onRedirectProfile(View v)
        {
            Intent nextActivity = new Intent(MainActivity.Instance, typeof(ProfileUserActivity));
            nextActivity.PutExtra("userid", Settings.GetTokketUser().Id);
            MainActivity.Instance.StartActivity(nextActivity);
        }

        [Java.Interop.Export("OnTapSearchTip")]
        public void OnTapSearchTip(View v)
        {
            search_fragment.Instance.framTip.Visibility = ViewStates.Gone;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent nextActivity;
            switch (item.ItemId)
            {
                //...

                //Handle sandwich menu icon click
                case Android.Resource.Id.Home:
                    //If menu is open, close it. Else, open it.
                    if (drawerLayout.IsDrawerOpen(GravityCompat.Start))
                        drawerLayout.CloseDrawers();
                    else
                        drawerLayout.OpenDrawer(GravityCompat.Start);
                    break;
                case Resource.Id.btnFilter: //Accessed from Home
                    int tabIndx = tabLayout.SelectedTabPosition;
                    switch (tabIndx)
                    {
                        case 0: //Home
                            Settings.MaintTabInt = (int)MainTab.Home;
                            break;
                        case 1: //Search
                            Settings.MaintTabInt = (int)MainTab.Search;
                            break;
#if (_TOKKEPEDIA)
                            case 2: //Notification
                            Settings.MaintTabInt = (int)MainTab.Notification;
                            break;
                        case 3: //Profile
                            break;
#endif
#if (_CLASSTOKS)
                        case 2: //Profile
                            break;
#endif
                        default:
                            Settings.MaintTabInt = (int)MainTab.Home;
                            break;
                    }
                    nextActivity = new Intent(this, typeof(FilterActivity));
                    if (Settings.MaintTabInt == (int)MainTab.Home)
                    {
                        nextActivity.PutExtra("activitycaller", "Home");
                    }
                    else if (Settings.MaintTabInt == (int)MainTab.Search)
                    {
                        nextActivity.PutExtra("activitycaller", "Search");
                    }
#if (_TOKKEPEDIA)
                    editor.PutString("TokModelList", JsonConvert.SerializeObject(HomeVm.TokDataList));
                    editor.Apply();
#endif
                    StartActivityForResult(nextActivity, (int)ActivityName.ActivityHome);

                    break;
                case Resource.Id.item_filter: //Accessed from Profile Tab
                    editor.PutString("TokModelList", JsonConvert.SerializeObject(profile_fragment.Instance.TokDataList));
                    editor.Apply();

                    Settings.MaintTabInt = (int)MainTab.Profile;
                    nextActivity = new Intent(this, typeof(FilterActivity));
                    nextActivity.PutExtra("activitycaller", "Profile");
                    nextActivity.PutExtra("SubTitle", "User: " + Settings.GetUserModel().DisplayName);
                    StartActivityForResult(nextActivity, (int)ActivityName.ActivityProfile);
                    break;
                case Resource.Id.item_avatar:
                    nextActivity = new Intent(this, typeof(AvatarsActivity));
                    this.StartActivityForResult(nextActivity, (int)ActivityType.AvatarsActivity);
                    break;
                case Resource.Id.item_sets:
                    nextActivity = new Intent(this, typeof(MySetsActivity));
                    nextActivity.PutExtra("isAddToSet", false);
                    nextActivity.PutExtra("TokTypeId", "");
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    StartActivity(nextActivity);
                    break;
                case Resource.Id.item_Share:
                    RunOnUiThread(async () => await Share.RequestAsync(new ShareTextRequest
                    {
                        Uri = Shared.Config.Configurations.Url + "user/" + ProfileVm.UserId,
                        Title = ProfileVm.UserDisplayName
                    }));

                    break;
                case Resource.Id.item_titles:
                    nextActivity = new Intent(this, typeof(ProfileTitleActivity));
                    StartActivity(nextActivity);
                    break;
                case Resource.Id.item_badges:
                    nextActivity = new Intent(this, typeof(BadgesActivity));
                    this.StartActivityForResult(nextActivity, 40011);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        private void M_navigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.MenuItem.ItemId)
            {
                case Resource.Id.leftmenu_TokGroup:
                    nextActivity = new Intent(this, typeof(TokGroupsActivity));
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    this.StartActivity(nextActivity);
                    break;

                case Resource.Id.leftmenu_Set:
                    Settings.ActivityInt = Convert.ToInt16(ActivityType.LeftMenuSets);
                    nextActivity = new Intent(this, typeof(MySetsActivity));
                    nextActivity.PutExtra("isAddToSet", false);
                    nextActivity.PutExtra("TokTypeId", "");
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    this.StartActivity(nextActivity);
                    break;
                case Resource.Id.leftmenu_ClassToks:
                    MainToolbar.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3498db"));
                    tabLayout.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3498db"));
                    ScrollNormalToks.Visibility = ViewStates.Gone;
                    ScrollClassToks.Visibility = ViewStates.Visible;
                    fragments.RemoveAt(0);
                    fragmentTitles.RemoveAt(0);
                    fragments.Insert(0, new classtoks_fragment(Settings.GetUserModel().UserId));
                    fragmentTitles.Insert(0, "Class Toks");

                    adapter = new AdapterStateFragmentX(SupportFragmentManager, fragments, fragmentTitles);
                    setupViewPager(MainViewPager);
                    setupClassTokTabIcons();
                    break;
                //case Resource.Id.leftmenu_Privacy:
                //    break;

                //case Resource.Id.leftmenu_Theme:
                //    break;
                case Resource.Id.leftmenu_Settings:
                    nextActivity = new Intent(this, typeof(SettingsActivity));
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    MainActivity.Instance.StartActivity(nextActivity);
                    break;

                //Class Toks Menu Below
                case Resource.Id.leftmenu_classtok_ClassSets:
                    Settings.ActivityInt = Convert.ToInt16(ActivityType.LeftMenuSets);
                    nextActivity = new Intent(this, typeof(MyClassSetsActivity));
                    nextActivity.PutExtra("isAddToSet", false);
                    nextActivity.PutExtra("TokTypeId", "");
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    this.StartActivity(nextActivity);
                    break;
                case Resource.Id.leftmenu_classtok_ClassGroups:
                    nextActivity = new Intent(this, typeof(ClassGroupListActivity));
                    MainActivity.Instance.StartActivity(nextActivity);
                    break;
                case Resource.Id.leftmenu_classtok_Invites:
                    break;
                case Resource.Id.leftmenu_classtok_Guide:
                    break;
                case Resource.Id.leftmenu_classtok_Folders:
                    break;
                case Resource.Id.leftmenu_classtok_Tokkepedia:
                    MainToolbar.SetBackgroundColor(Android.Graphics.Color.ParseColor("#b085ed"));
                    tabLayout.SetBackgroundColor(Android.Graphics.Color.ParseColor("#b085ed"));
                    m_navigationView.Menu.Clear();
                    ScrollNormalToks.Visibility = ViewStates.Visible;
                    ScrollClassToks.Visibility = ViewStates.Gone;
                    //m_navigationView.InflateMenu(Resource.Menu.left_menus);

                    GetQuoteOfTheHour(m_navigationView);

                    fragments.RemoveAt(0);
                    fragmentTitles.RemoveAt(0);
                    fragments.Insert(0, new home_fragment());
                    fragmentTitles.Insert(0, "Home");
                    fragments.Insert(1, new search_fragment());
                    fragmentTitles.Insert(1, "Search");
                    fragments.Insert(2, new notification_fragment());
                    fragmentTitles.Insert(2, "Notifications");
                    fragments.Insert(3, new profile_fragment());
                    fragmentTitles.Insert(3, "Profile");


                    adapter = new AdapterStateFragmentX(SupportFragmentManager, fragments, fragmentTitles);
                    setupViewPager(MainViewPager);
                    setupHomeTabIcons();


                    //change header layout bg color for tokkepedia
                    var headerlayout = FindViewById(Resource.Id.relativeLayoutHeader);
                    headerlayout.SetBackgroundColor(Android.Graphics.Color.ParseColor("#b085ed"));
                    break;
            }
            e.MenuItem.SetChecked(false);
            drawerLayout.CloseDrawer(GravityCompat.Start);
        }

        [Java.Interop.Export("OnClickClassMenu")]
        public void OnClickClassMenu(View v)
        {
            switch (v.Id)
            {
                //Class Toks Menu Below
                case Resource.Id.leftmenu_classtok_ClassSets:
                    Settings.ActivityInt = Convert.ToInt16(ActivityType.LeftMenuSets);
                    nextActivity = new Intent(this, typeof(MyClassSetsActivity));
                    nextActivity.PutExtra("isAddToSet", false);
                    nextActivity.PutExtra("TokTypeId", "");
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    this.StartActivity(nextActivity);
                    break;
                case Resource.Id.leftmenu_classtok_ClassGroups:
                    nextActivity = new Intent(this, typeof(ClassGroupListActivity));
                    MainActivity.Instance.StartActivity(nextActivity);
                    break;
                case Resource.Id.leftmenu_classtok_Invites:
                    break;
                case Resource.Id.leftmenu_classtok_Guide:
                    break;
                case Resource.Id.leftmenu_classtok_Folders:
                    break;
                case Resource.Id.leftmenu_classtok_Tokkepedia:
                    MainToolbar.SetBackgroundColor(Android.Graphics.Color.ParseColor("#b085ed"));
                    tabLayout.SetBackgroundColor(Android.Graphics.Color.ParseColor("#b085ed"));
                    m_navigationView.Menu.Clear();
                    ScrollNormalToks.Visibility = ViewStates.Visible;
                    ScrollClassToks.Visibility = ViewStates.Gone;
                    //m_navigationView.InflateMenu(Resource.Menu.left_menus);

                    GetQuoteOfTheHour(m_navigationView);

                    fragments.RemoveAt(0);
                    fragmentTitles.RemoveAt(0);
                    fragments.Insert(0, new home_fragment());
                    fragmentTitles.Insert(0, "Home");
                    fragments.Insert(1, new search_fragment());
                    fragmentTitles.Insert(1, "Search");
                    fragments.Insert(2, new notification_fragment());
                    fragmentTitles.Insert(2, "Notifications");
                    fragments.Insert(3, new profile_fragment());
                    fragmentTitles.Insert(3, "Profile");

                    adapter = new AdapterStateFragmentX(SupportFragmentManager, fragments, fragmentTitles);
                    setupViewPager(MainViewPager);
                    setupHomeTabIcons();


                    //change header layout bg color for tokkepedia
                    var headerlayout = FindViewById(Resource.Id.relativeLayoutHeader);
                    headerlayout.SetBackgroundColor(Android.Graphics.Color.ParseColor("#b085ed"));
                    break;
            }
            drawerLayout.CloseDrawers();
        }

        [Java.Interop.Export("OnClickCustomMenu")]
        public void OnClickCustomMenu(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.LinearTokGroups:
                    nextActivity = new Intent(this, typeof(TokGroupsActivity));
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    this.StartActivity(nextActivity);
                    drawerLayout.CloseDrawers();
                    break;
                case Resource.Id.LinearSet:
                    Settings.ActivityInt = Convert.ToInt16(ActivityType.LeftMenuSets);
                    nextActivity = new Intent(this, typeof(MySetsActivity));
                    nextActivity.PutExtra("isAddToSet", false);
                    nextActivity.PutExtra("TokTypeId", "");
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    this.StartActivity(nextActivity);
                    drawerLayout.CloseDrawers();
                    break;
                case Resource.Id.LinearClassToks:
                    MainToolbar.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3498db"));
                    tabLayout.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3498db"));
                    ScrollNormalToks.Visibility = ViewStates.Gone;
                    ScrollClassToks.Visibility = ViewStates.Visible;

                    fragments.RemoveAt(0);
                    fragmentTitles.RemoveAt(0);
                    fragments.Insert(0, new classtoks_fragment(Settings.GetUserModel().UserId));
                    fragmentTitles.Insert(0, "Class Toks");

                    if (fragmentTitles.Count == 4)
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            fragments.RemoveAt(1);
                            fragmentTitles.RemoveAt(1);
                        }
                    }


                    adapter = new AdapterStateFragmentX(SupportFragmentManager, fragments, fragmentTitles);
                    setupViewPager(MainViewPager);
                    setupClassTokTabIcons();

                    drawerLayout.CloseDrawers();


                    //change header layout bg color for classtok
                    var headerlayout = FindViewById(Resource.Id.relativeLayoutHeader);
                    headerlayout.SetBackgroundColor(Android.Graphics.Color.ParseColor("#3498db"));

                    break;
                case Resource.Id.LinearSettings:
                    nextActivity = new Intent(this, typeof(SettingsActivity));
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    this.StartActivity(nextActivity);
                    drawerLayout.CloseDrawers();
                    break;
                case Resource.Id.LinearClassTokSettings:
                    nextActivity = new Intent(this, typeof(SettingsActivity));
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    this.StartActivity(nextActivity);
                    drawerLayout.CloseDrawers();
                    break;
                case Resource.Id.LinearQuoteTitle:
                    break;
                case Resource.Id.BtnMenuAddTok:
                    nextActivity = new Intent(this, typeof(AddTokActivity));
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    this.StartActivity(nextActivity);
                    break;
            }
        }

        [Java.Interop.Export("OnClickQuotePage")]
        public void OnClickQuotePage(View v)
        {
            Intent nextActivity = new Intent(this, typeof(QuoteActivity));
            this.StartActivity(nextActivity);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == (int)ActivityType.ProfileTabActivity) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
                Intent nextActivity = new Intent(this, typeof(CropImageActivity));
                Settings.ImageBrowseCrop = (string)uri;
                this.StartActivityForResult(nextActivity, requestCode);
            }
            else if ((requestCode == 40011) && (resultCode == Android.App.Result.Ok)) //Badge accessed from Profile Tab
            {
                var badgeString = data.GetStringExtra("Badge");
                var badgeModel = JsonConvert.DeserializeObject<BadgeOwned>(badgeString);
                Glide.With(this).Load(badgeModel.Image).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(profile_fragment.Instance.ProfileUserPhoto);
            }
            else if (requestCode == (int)ActivityName.ActivityHome && resultCode == Android.App.Result.Ok) //Filter
            {
#if (_TOKKEPEDIA)
                this.RunOnUiThread(async () => await home_fragment.HFInstance.InitAsync((FilterType)Settings.FilterTag, Settings.SortByFilter));
#endif

#if (_CLASSTOKS)
                if (Settings.MaintTabInt == (int)MainTab.Search)
                {
                    if (Settings.ClassSearchFilter == (int)FilterType.All)
                    {
                       search_fragment.Instance.SearchText.Hint = "Search in Public Class Toks";
                    }
                    else if (Settings.ClassSearchFilter == (int)FilterType.Featured)
                    {
                        search_fragment.Instance.SearchText.Hint = "Search in My Class Toks";
                    }
                    classtoks_fragment.Instance.isSearchFragment = true;
                }
                else
                {
                    classtoks_fragment.Instance.isSearchFragment = false;
                }


                fragments.RemoveAt(0);
                fragmentTitles.RemoveAt(0);
                fragments.Insert(0, new classtoks_fragment(Settings.GetUserModel().UserId));
                fragmentTitles.Insert(0, "Class Toks");

                adapter = new AdapterStateFragmentX(SupportFragmentManager, fragments, fragmentTitles);
                setupViewPager(MainViewPager);
                setupClassTokTabIcons();
                //this.RunOnUiThread(async () => await classtoks_fragment.Instance.InitializeData());
#endif
            }
            else if (requestCode == (int)ActivityName.ActivityProfile && resultCode == Android.App.Result.Ok) //Filter
            {
                this.RunOnUiThread(async () => await profile_fragment.Instance.LoadToks());
            }
        }
        public void displayImageBrowse()
        {
            byte[] imageMainBytes = Convert.FromBase64String(Settings.ImageBrowseCrop);
            if (Settings.BrowsedImgTag == 0) //UserPhoto
            {
                profile_fragment.Instance.ProfileUserPhoto.SetImageBitmap((BitmapFactory.DecodeByteArray(imageMainBytes, 0, imageMainBytes.Length)));
            }
            else if (Settings.BrowsedImgTag == 1) //Cover Photo
            {
                profile_fragment.Instance.ProfileCoverPhoto.SetImageBitmap((BitmapFactory.DecodeByteArray(imageMainBytes, 0, imageMainBytes.Length)));
            }

            this.RunOnUiThread(async () => await SaveUserCoverPhoto(Settings.ImageBrowseCrop));
            Settings.ImageBrowseCrop = null;
        }
        public async Task SaveUserCoverPhoto(string base64img)
        {
            base64img = "data:image/jpeg;base64," + base64img;
            if (Settings.BrowsedImgTag == 0) //UserPhoto
            {
                await ServiceAccount.AccountService.Instance.UploadProfilePictureAsync(base64img);
            }
            else if (Settings.BrowsedImgTag == 1) //Cover Photo
            {
                await ServiceAccount.AccountService.Instance.UploadProfileCoverAsync(base64img);
            }
        }
        private void Viewpager_PageSelected(object sender, AndroidX.ViewPager.Widget.ViewPager.PageSelectedEventArgs e)
        {
            //set color status bar
            this.Window.SetStatusBarColor(Android.Graphics.Color.Transparent);

            if (Settings.CurrentTheme == 0)
            {
                //set color action bar
                this.SupportActionBar.SetBackgroundDrawable(new ColorDrawable(defaultPrimaryColor));
                this.tabLayout.SetBackgroundColor(defaultPrimaryColor);

            }
            else
            {
                //set color action bar
                this.SupportActionBar.SetBackgroundDrawable(new ColorDrawable(defaultPrimaryDarkerColor));
                this.tabLayout.SetBackgroundColor(defaultPrimaryDarkerColor);
            }
            
            //Display appbar if hidden
            MainAppBar.SetExpanded(true, true);

            //InvalidateOptionsMenu and call OnPrepareOptionMenu to update Menu
            switch (e.Position)
            {
                case 0:
                    ViewCompat.RequestApplyInsets(MainAppBar);
                    Settings.ActivityInt = (int)ActivityType.HomePage;
                    InvalidateOptionsMenu();
                    break;
                case 1://Search
                    Settings.ActivityInt = (int)ActivityType.TokSearch;
                    InvalidateOptionsMenu();
                    break;
#if (_TOKKEPEDIA)
                    case 2: //Notification
                    InvalidateOptionsMenu();
                    break;
                case 3:
                    //When profile tab is selected
                    Settings.ActivityInt = (int)ActivityType.ProfileTabActivity;
                    InvalidateOptionsMenu();
                    break;
#endif
#if (_CLASSTOKS)
                case 2:
                    //When profile tab is selected
                    Settings.ActivityInt = (int)ActivityType.ProfileTabActivity;
                    InvalidateOptionsMenu();
                    break;
#endif
            }
        }
        private async void GetQuoteOfTheHour(NavigationView m_navigationView)
        {
            var QuoteTitle = (TextView)m_navigationView.FindViewById(Resource.Id.lblQuote);
            var QuoteAuthor = (TextView)m_navigationView.FindViewById(Resource.Id.lblQuoteAuthor);
            var QuoteCategory = (TextView)m_navigationView.FindViewById(Resource.Id.lblQuoteCategory);
            var QuoteProgress = (ProgressBar)m_navigationView.FindViewById(Resource.Id.progressQuote);

            OggClass sendItem = await ServiceAccount.CommonService.Instance.GetQuoteAsync();
            QuoteTitle.Text = sendItem.PrimaryText;
            QuoteAuthor.Text = sendItem.SecondaryText;
            QuoteCategory.Text = "Category: " + sendItem.Category;
            QuoteProgress.Visibility = ViewStates.Gone;

        }
        public AndroidX.AppCompat.Widget.Toolbar MainToolbar => FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.main_toolbar);
        public FloatingActionButton fab_AddTok => FindViewById<FloatingActionButton>(Resource.Id.fab_AddTok);
        public CollapsingToolbarLayout CollapsingToolBarMain => FindViewById<CollapsingToolbarLayout>(Resource.Id.CollapsingToolBarMain);
        public ScrollView ScrollNormalToks => FindViewById<ScrollView>(Resource.Id.ScrollNormalToks);
        public ScrollView ScrollClassToks => FindViewById<ScrollView>(Resource.Id.ScrollClassToks);
        public LinearLayout LinearCoinsToast => m_navigationView.GetHeaderView(0).FindViewById<LinearLayout>(Resource.Id.LinearCoinsToast);
        public TextView TextCoinsToast => m_navigationView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.TextCoinsToast);
        public TextView txtGroup => m_navigationView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.lblGroup);
        public ImageView BulbImg => FindViewById<ImageView>(Resource.Id.imageBulb);
        public ImageView imageViewLogoText => FindViewById<ImageView>(Resource.Id.imageViewTokkepediaLogoText);
        public AppBarLayout MainAppBar => FindViewById<AppBarLayout>(Resource.Id.appbarMain);

    }
}