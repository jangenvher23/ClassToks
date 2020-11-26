using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using Newtonsoft.Json;
using Tokkepedia.Fragments;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.ViewModels;

namespace Tokkepedia
{
#if (_TOKKEPEDIA)
    [Activity(Label = "Filter", Theme = "@style/CustomAppTheme")]
#endif

#if (_CLASSTOKS)
    [Activity(Label = "Filter", Theme = "@style/CustomAppThemeBlue")]
#endif

    public class FilterActivity : BaseActivity
    {
        string activitycaller = "", SubTitle = "", selectedColor = "#b085ed";
        Color primaryColor;
        bool IsChanged = false;
        public List<TokModel> ListTokModel;
        int filterToks = (int)FilterToks.Toks;
        internal static FilterActivity Instance { get; private set; }
        private int REQUEST_CLASS_FILTER = 1001;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.filter_page);

            Instance = this;

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            activitycaller = Intent.GetStringExtra("activitycaller");
            if (activitycaller == null)
                activitycaller = "";

            SubTitle = Intent.GetStringExtra("SubTitle");

            if (Settings.ActivityInt == (int)ActivityType.TokSearch)
            {
                linearImage.Visibility = ViewStates.Gone;
                linearPlay.Visibility = ViewStates.Gone;
                linearSortBy.Visibility = ViewStates.Gone;

#if (_TOKKEPEDIA)
                LinearFeed.Visibility = ViewStates.Gone;
                primaryColor = new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.primary_dark));
#endif

#if (_CLASSTOKS)
                LinearFeed.Visibility = ViewStates.Visible;
#endif

                linearGroup.Visibility = ViewStates.Gone;
            }
            else if (Settings.ActivityInt == (int)ActivityType.ProfileTabActivity)
            {
                LinearFeed.Visibility = ViewStates.Gone;
            }
            else
            {
                linearImage.Visibility = ViewStates.Visible;
                linearPlay.Visibility = ViewStates.Visible;
                linearSortBy.Visibility = ViewStates.Visible;
                LinearFeed.Visibility = ViewStates.Visible;
                linearGroup.Visibility = ViewStates.Visible;
            }


            if (activitycaller.ToUpper() == "TOKSACTIVITY")
            {
                LinearFeed.Visibility = ViewStates.Gone;
            }


            if (Settings.ActivityInt == (int)ActivityType.ClassGroupActivity || Settings.ActivityInt == (int)ActivityType.ClassGroupListActivity)
            {
                linearGroup.Visibility = ViewStates.Visible;
            }
            else
            {
                linearGroup.Visibility = ViewStates.Gone;
            }

#if (_CLASSTOKS)
            LinearFilterBy.Visibility = ViewStates.Visible;
            btnGlobalToks.Text = "Public";
            btnFeaturedToks.Text = "My Class Toks";
            selectedColor = "#78bff0";
            primaryColor = new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent));
            setColorBtn();
#endif

#if (_TOKKEPEDIA)
            LinearFilterBy.Visibility = ViewStates.Gone;
            if (Settings.FilterFeed == (int)FilterType.All)
            {
                btnGlobalToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }
            else if(Settings.FilterFeed == (int)FilterType.Featured)
            {
                btnFeaturedToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }
#endif

#if (_CLASSTOKS)
            if (activitycaller.ToUpper() == "HOME")
            {
                if (Settings.ClassHomeFilter == (int)FilterType.All)
                {
                    btnGlobalToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                }
                else
                {
                    btnFeaturedToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                }
            }
            else if (activitycaller.ToUpper() == "SEARCH")
            {
                if (Settings.ClassSearchFilter == (int)FilterType.All)
                {
                    btnGlobalToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                }
                else
                {
                    btnFeaturedToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                }
            }
#endif

            //View As
            if (Settings.MaintTabInt == (int)MainTab.Home)
            {
                filterToks = Settings.FilterToksHome;
            }
            else if (Settings.MaintTabInt == (int)MainTab.Search)
            {
                filterToks = Settings.FilterToksSearch;
            }
            else if (Settings.MaintTabInt == (int)MainTab.Profile)
            {
                filterToks = Settings.FilterToksProfile;
            }

            if (filterToks == (int)FilterToks.Toks)
            {
                btnFilterToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }
            else if (filterToks == (int)FilterToks.Cards)
            {
                btnFilterCards.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }
            
            //Sort By
            if (Settings.SortByFilter == "standard")
            {
                Settings.SortByFilterTag = (int)FilterType.Standard;
                btnStandard.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }
            else if (Settings.SortByFilter == "recent")
            {
                Settings.SortByFilterTag = (int)FilterType.Recent;
                btnRecent.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }
            else if (Settings.SortByFilter == "toptoks")
            {
                btnTopToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }

            //Image
            if (Settings.FilterImage == (int)ImageType.Image)
            {
                btnFilterImage.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }
            else if (Settings.FilterImage == (int)ImageType.NonImage)
            {
                btnFilterNoImage.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }
            else if (Settings.FilterImage == (int)ImageType.Both)
            {
                btnFilterBoth.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }

            //Group
            if (Settings.FilterGroup == (int)GroupFilter.OwnGroup)
            {
                btnFilterOwnedGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }
            else if (Settings.FilterGroup == (int)GroupFilter.JoinedGroup)
            {
                btnFilterJoinedGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }
            else if (Settings.FilterGroup == (int)GroupFilter.MyGroup)
            {
                btnFilterMyGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
            }

            //FilterBy
            setBtnFilterByClick();

            //Feed
            btnGlobalToks.Click += btnFilterClick;
            btnFeaturedToks.Click += btnFilterClick;

            //View As
            btnFilterToks.Click += btnFilterClick;
            btnFilterCards.Click += btnFilterClick;

            //Sort
            btnStandard.Click += btnFilterClick;
            btnTopToks.Click += btnFilterClick;
            btnRecent.Click += btnFilterClick;

            //Image
            btnFilterImage.Click += btnFilterClick;
            btnFilterNoImage.Click += btnFilterClick;
            btnFilterBoth.Click += btnFilterClick;

            //Group
            btnFilterMyGroup.Click += btnFilterClick;
            btnFilterJoinedGroup.Click += btnFilterClick;
            btnFilterOwnedGroup.Click += btnFilterClick;

            //Play
            btnFilterTokCards.Click -= btnFilterTokCardsClick;
            btnFilterTokCards.Click += btnFilterTokCardsClick;

            
            btnFilterByAll.Click += delegate
            {
                if (activitycaller.ToUpper() == "HOME")
                {
                    Settings.FilterByTypeHome = (int)FilterBy.None;
                    Settings.FilterByItemsHome = "";
                }
                else if (activitycaller.ToUpper() == "SEARCH")
                {
                    Settings.FilterByTypeSearch = (int)FilterBy.None;
                    Settings.FilterByItemsSearch = "";
                }
                else if (activitycaller.ToUpper() == "PROFILE")
                {
                    Settings.FilterByTypeProfile = (int)FilterBy.None;
                    Settings.FilterByItemsProfile = "";
                }

                setBtnFilterByClick();
                IsChanged = true;
            };

            btnClass.ContentDescription = "Class";
            btnClass.Click += btnFilterBy;

            btnCategory.ContentDescription = "Category";
            btnCategory.Click += btnFilterBy;

            btnType.ContentDescription = "Type";
            btnType.Click += btnFilterBy;

            btnFilterTokMatch.Click += (object sender, EventArgs e) =>
            {
                Intent nextActivity = new Intent(this, typeof(TokMatchActivity));
                nextActivity.PutExtra("isSet", false);
                nextActivity.PutExtra("SubTitle", SubTitle);
                this.StartActivity(nextActivity);
            };

            BtnTokChoice.Click += (object sender, EventArgs e) =>
            {
                Intent nextActivity = new Intent(this, typeof(TokChoiceActivity));
                nextActivity.PutExtra("isSet", false);
                nextActivity.PutExtra("SubTitle", SubTitle);
                this.StartActivity(nextActivity);
            };

            btnFilterSets.Click += (object sender, EventArgs e) =>
            {
                Intent nextActivity = new Intent(this, typeof(MySetsActivity));
                nextActivity.PutExtra("isAddToSet", false);
                nextActivity.PutExtra("TokTypeId", "");
                this.StartActivity(nextActivity);
            };
        }

        private void btnFilterBy(object sender, EventArgs e)
        {
            Intent nextActivity = new Intent(this, typeof(ClassFilterbyActivity));
            nextActivity.PutExtra("filterby", (sender as Button).ContentDescription);
            nextActivity.PutExtra("caller", activitycaller.ToLower());
            this.StartActivityForResult(nextActivity, REQUEST_CLASS_FILTER);
        }
        
        private void btnFilterTokCardsClick(object sender, EventArgs e)
        {
            Intent nextActivity = new Intent(this, typeof(TokCardsMiniGameActivity));
            nextActivity.PutExtra("isSet", false);
            nextActivity.PutExtra("SubTitle", SubTitle);
            this.StartActivity(nextActivity);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                //...

                //Handle sandwich menu icon click
                case Android.Resource.Id.Home:
                    if (IsChanged)
                    {
                        ApplyFilter();
                    }
                    
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        private void setBtnFilterByClick()
        {
            if (activitycaller.ToUpper() == "HOME")
            {
                if (Settings.FilterByTypeHome == (int)FilterBy.None)
                {
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
                else if (Settings.FilterByTypeHome == (int)FilterBy.Class)
                {
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
                else if (Settings.FilterByTypeHome == (int)FilterBy.Category)
                {
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
                else if (Settings.FilterByTypeHome == (int)FilterBy.Type)
                {
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
            }
            else if (activitycaller.ToUpper() == "SEARCH")
            {
                if (Settings.FilterByTypeSearch == (int)FilterBy.None)
                {
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
                else if (Settings.FilterByTypeSearch == (int)FilterBy.Class)
                {
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
                else if (Settings.FilterByTypeSearch == (int)FilterBy.Category)
                {
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
                else if (Settings.FilterByTypeSearch == (int)FilterBy.Type)
                {
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
            }
            else if (activitycaller.ToUpper() == "PROFILE")
            {
                if (Settings.FilterByTypeProfile == (int)FilterBy.None)
                {
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
                else if (Settings.FilterByTypeProfile == (int)FilterBy.Class)
                {
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
                else if (Settings.FilterByTypeProfile == (int)FilterBy.Category)
                {
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
                else if (Settings.FilterByTypeProfile == (int)FilterBy.Type)
                {
                    btnType.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                    btnClass.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnCategory.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                    btnFilterByAll.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
                }
            }
        }

        private void btnFilterClick(object sender, EventArgs e)
        {
            Button btnClicked = (Button)sender;

            if ((sender as Button).ContentDescription == "selected")
            {
                //(sender as Button).ContentDescription = "";
                //(sender as Button).BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else
            {
                (sender as Button).BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor(selectedColor));
                (sender as Button).ContentDescription = "selected";
            }

            if (btnClicked.Text.ToLower() == "global toks" || btnClicked.Text.ToLower() == "public")
            {
                if (Settings.FilterFeed != (int)FilterType.All)
                {
                    IsChanged = true;
                }
                Settings.FilterFeed = (int)FilterType.All;
                Settings.FilterTag = (int)FilterType.All;

#if (_CLASSTOKS)
                if (activitycaller.ToUpper() == "HOME")
                {
                    Settings.FilterByTypeHome = (int)FilterBy.None;
                    Settings.FilterByItemsHome = "";
                    Settings.ClassHomeFilter = (int)FilterType.All;
                }
                else if (activitycaller.ToUpper() == "SEARCH")
                {
                    Settings.FilterByTypeSearch = (int)FilterBy.None;
                    Settings.FilterByItemsSearch = "";
                    Settings.ClassSearchFilter = (int)FilterType.All;
                }
#endif

                btnFeaturedToks.ContentDescription = "";
                btnFeaturedToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else if (btnClicked.Text.ToLower() == "featured toks" || btnClicked.Text.ToLower() == "my class toks")
            {
                if (Settings.FilterFeed != (int)FilterType.Featured)
                {
                    IsChanged = true;
                }
                Settings.FilterFeed = (int)FilterType.Featured;
                Settings.FilterTag = (int)FilterType.Featured;

#if (_CLASSTOKS)
                if (activitycaller.ToUpper() == "HOME")
                {
                    Settings.ClassHomeFilter = (int)FilterType.Featured;
                    Settings.FilterByTypeHome = (int)FilterBy.None;
                    Settings.FilterByItemsHome = "";
                }
                else if (activitycaller.ToUpper() == "SEARCH")
                {
                    Settings.ClassSearchFilter = (int)FilterType.Featured;
                    Settings.FilterByTypeSearch = (int)FilterBy.None;
                    Settings.FilterByItemsSearch = "";
                }
#endif

                btnGlobalToks.ContentDescription = "";
                btnGlobalToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else if (btnClicked.Text.ToLower() == "toks")
            {
                if (filterToks != (int)FilterToks.Toks)
                {
                    IsChanged = true;
                }
                setFilterToks((int)FilterToks.Toks);

                btnFilterCards.ContentDescription = "";
                btnFilterCards.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else if (btnClicked.Text.ToLower() == "cards")
            {
                if (filterToks != (int)FilterToks.Cards)
                {
                    IsChanged = true;
                }
                setFilterToks((int)FilterToks.Cards);

                btnFilterToks.ContentDescription = "";
                btnFilterToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else if (btnClicked.Text.ToLower() == "standard")
            {
                if (Settings.SortByFilter != "standard")
                {
                    IsChanged = true;
                }

                Settings.FilterTag = (int)FilterType.Standard;
                Settings.SortByFilter = "standard";

                btnRecent.ContentDescription = "";
                btnRecent.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);

                btnTopToks.ContentDescription = "";
                btnTopToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else if (btnClicked.Text.ToLower() == "recent")
            {
                if (Settings.SortByFilter != "recent")
                {
                    IsChanged = true;
                }

                Settings.FilterTag = (int)FilterType.Recent;
                Settings.SortByFilter = "recent";

                btnStandard.ContentDescription = "";
                btnStandard.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);

                btnTopToks.ContentDescription = "";
                btnTopToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else if (btnClicked.Text.ToLower() == "top toks")
            {
                //No Code for Top Toks yet

                btnStandard.ContentDescription = "";
                btnStandard.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);

                btnRecent.ContentDescription = "";
                btnRecent.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }

            //Image
            if (btnClicked.Text.ToLower() == "image")
            {
                if (Settings.FilterImage != (int)ImageType.Image)
                {
                    IsChanged = true;
                }

                Settings.FilterImage = (int)ImageType.Image;

                btnFilterNoImage.ContentDescription = "";
                btnFilterNoImage.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);

                btnFilterBoth.ContentDescription = "";
                btnFilterBoth.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else if (btnClicked.Text.ToLower() == "no image")
            {
                if (Settings.FilterImage != (int)ImageType.NonImage)
                {
                    IsChanged = true;
                }

                Settings.FilterImage = (int)ImageType.NonImage;

                btnFilterImage.ContentDescription = "";
                btnFilterImage.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);

                btnFilterBoth.ContentDescription = "";
                btnFilterBoth.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else if (btnClicked.Text.ToLower() == "both")
            {
                if (Settings.FilterImage != (int)ImageType.Both)
                {
                    IsChanged = true;
                }

                Settings.FilterImage = (int)ImageType.Both;

                btnFilterNoImage.ContentDescription = "";
                btnFilterNoImage.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);

                btnFilterImage.ContentDescription = "";
                btnFilterImage.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else if (btnClicked.Text.ToLower() == "my groups")
            {
                if (Settings.FilterGroup != (int)GroupFilter.MyGroup)
                {
                    IsChanged = true;
                }

                Settings.FilterGroup = (int)GroupFilter.MyGroup;

                btnFilterJoinedGroup.ContentDescription = "";
                btnFilterJoinedGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);

                btnFilterOwnedGroup.ContentDescription = "";
                btnFilterOwnedGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else if (btnClicked.Text.ToLower() == "joined groups")
            {
                if (Settings.FilterGroup != (int)GroupFilter.JoinedGroup)
                {
                    IsChanged = true;
                }

                Settings.FilterGroup = (int)GroupFilter.JoinedGroup;

                btnFilterMyGroup.ContentDescription = "";
                btnFilterMyGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);

                btnFilterOwnedGroup.ContentDescription = "";
                btnFilterOwnedGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
            else if (btnClicked.Text.ToLower() == "owned groups")
            {
                if (Settings.FilterGroup != (int)GroupFilter.OwnGroup)
                {
                    IsChanged = true;
                }

                Settings.FilterGroup = (int)GroupFilter.OwnGroup;

                btnFilterMyGroup.ContentDescription = "";
                btnFilterMyGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);

                btnFilterJoinedGroup.ContentDescription = "";
                btnFilterJoinedGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(primaryColor);
            }
        }

        private void ApplyFilter()
        {
            Intent = new Intent();
            SetResult(Android.App.Result.Ok, Intent);
            Finish();
        }
        private void setFilterToks(int ftoks)
        {
            if (Settings.MaintTabInt == (int)MainTab.Home)
            {
                Settings.FilterToksHome = ftoks;
            }
            else if (Settings.MaintTabInt == (int)MainTab.Search)
            {
                Settings.FilterToksSearch = ftoks;
            }
            else if (Settings.MaintTabInt == (int)MainTab.Profile)
            {
                Settings.FilterToksProfile = ftoks;
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if ((requestCode == REQUEST_CLASS_FILTER) && (resultCode == Android.App.Result.Ok) && (data != null))
            {
                if (activitycaller.ToUpper() == "HOME")
                {
                    Settings.FilterByTypeHome = data.GetIntExtra("filterby", 0);
                    Settings.FilterByItemsHome = data.GetStringExtra("filterByList");

                    if ((FilterBy)Settings.FilterByTypeHome == FilterBy.Type)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(Settings.FilterByItemsHome);
                        if (items.Count == 1)
                        {
                            Settings.FilterByTypeSelectedHome = items[0];
                        }
                        else
                        {
                            Settings.FilterByTypeSelectedHome = "";
                        }
                    }
                }
                else if (activitycaller.ToUpper() == "SEARCH")
                {
                    Settings.FilterByTypeSearch = data.GetIntExtra("filterby", 0);
                    Settings.FilterByItemsSearch = data.GetStringExtra("filterByList");

                    if ((FilterBy)Settings.FilterByTypeSearch == FilterBy.Type)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(Settings.FilterByItemsSearch);
                        if (items.Count == 1)
                        {
                            Settings.FilterByTypeSelectedSearch = items[0];
                        }
                        else
                        {
                            Settings.FilterByTypeSelectedSearch = "";
                        }
                    }
                }
                else if (activitycaller.ToUpper() == "PROFILE")
                {
                    Settings.FilterByTypeProfile = data.GetIntExtra("filterby", 0);
                    Settings.FilterByItemsProfile = data.GetStringExtra("filterByList");

                    if ((FilterBy)Settings.FilterByTypeProfile == FilterBy.Type)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(Settings.FilterByItemsProfile);
                        if (items.Count == 1)
                        {
                            Settings.FilterByTypeSelectedProfile = items[0];
                        }
                        else
                        {
                            Settings.FilterByTypeSelectedProfile = "";
                        }
                    }
                }

                setBtnFilterByClick();
                IsChanged = true;
            }
        }

        private void setColorBtn()
        {
            btnGlobalToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));
            btnFeaturedToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));
            btnFilterToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));
            btnFilterCards.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnFilterMyGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnFilterJoinedGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnFilterOwnedGroup.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnStandard.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnRecent.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnTopToks.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnFilterImage.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnFilterNoImage.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnFilterBoth.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnFilterSets.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnFilterTokCards.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            btnFilterTokMatch.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
            BtnTokChoice.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));;
        }

        public Button btnGlobalToks => FindViewById<Button>(Resource.Id.btnGlobalToks);
        public Button btnFeaturedToks => FindViewById<Button>(Resource.Id.btnFeaturedToks);
        public Button btnTopToks => FindViewById<Button>(Resource.Id.btnTopToks);
        public Button btnStandard => FindViewById<Button>(Resource.Id.btnFilterStandard);
        public Button btnRecent => FindViewById<Button>(Resource.Id.btnRecent);
        public Button btnFilterToks => FindViewById<Button>(Resource.Id.btnFilterToks);
        public Button btnFilterSets => FindViewById<Button>(Resource.Id.btnFilterSets);
        public Button btnFilterCards => FindViewById<Button>(Resource.Id.btnFilterCards);
        public Button btnFilterTokCards => FindViewById<Button>(Resource.Id.btnFilterTokCards);
        public Button btnFilterTokMatch => FindViewById<Button>(Resource.Id.btnFilterTokMatch);
        public Button btnFilterImage => FindViewById<Button>(Resource.Id.btnFilterImage);
        public Button btnFilterNoImage => FindViewById<Button>(Resource.Id.btnFilterNoImage);
        public Button btnFilterBoth => FindViewById<Button>(Resource.Id.btnFilterBoth);
        public LinearLayout LinearFeed => FindViewById<LinearLayout>(Resource.Id.LinearFeed);
        public Button BtnTokChoice => FindViewById<Button>(Resource.Id.btnFilterTokChoice);

        public Button btnFilterMyGroup => FindViewById<Button>(Resource.Id.btnFilterMyGroup);
        public Button btnFilterJoinedGroup => FindViewById<Button>(Resource.Id.btnFilterJoinedGroup);
        public Button btnFilterOwnedGroup => FindViewById<Button>(Resource.Id.btnFilterOwnedGroup);
        public LinearLayout linearGroup => FindViewById<LinearLayout>(Resource.Id.linearGroup);
        public LinearLayout linearSortBy => FindViewById<LinearLayout>(Resource.Id.linearSortBy);
        public LinearLayout linearImage => FindViewById<LinearLayout>(Resource.Id.linearImage);
        public LinearLayout linearPlay => FindViewById<LinearLayout>(Resource.Id.linearPlay);
        public LinearLayout LinearFilterBy => FindViewById<LinearLayout>(Resource.Id.LinearFilterBy);
        public Button btnFilterByAll => FindViewById<Button>(Resource.Id.btnFilterByAll);
        public Button btnClass => FindViewById<Button>(Resource.Id.btnClass);
        public Button btnCategory => FindViewById<Button>(Resource.Id.btnCategory);
        public Button btnType => FindViewById<Button>(Resource.Id.btnType);
    }
}