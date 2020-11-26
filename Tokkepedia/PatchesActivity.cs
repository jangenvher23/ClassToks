using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Tokkepedia.Adapters;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Fragments;
using XFragment = AndroidX.Fragment.App.Fragment;
using XFragmentManager = AndroidX.Fragment.App.FragmentManager;
using Tokkepedia.Shared.Models;
using AndroidX.ViewPager.Widget;
using Google.Android.Material.Tabs;

namespace Tokkepedia
{
#if (_TOKKEPEDIA)
    [Activity(Label = "", Theme = "@style/CustomAppTheme")]
#endif

#if (_CLASSTOKS)
    [Activity(Label = "", Theme = "@style/CustomAppThemeBlue")]
#endif

    public class PatchesActivity : BaseActivity
    {
        List<PointsSymbolModel> ListPatchesColor;
        TokketUser TokketUserCur;
        internal static PatchesActivity Instance { get; private set; }
        string UserId;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.patchespage);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            Instance = this;

            TokketUserCur = Settings.GetTokketUser();
            UserId = TokketUserCur.Id;

            if (!string.IsNullOrEmpty(TokketUserCur.AccountType))
            {
                if (TokketUserCur.AccountType == "group")
                {
                    this.Title = TokketUserCur.Points + " points";
                }
                else
                {
                    this.Title = TokketUserCur.Points + " points";
                }
            }
            else
            {
                this.Title = TokketUserCur.Points + " points";
            }

            setupViewPager(ViewPagerPatches);
            TabPatches.SetupWithViewPager(ViewPagerPatches);

            BtnChangePatchColor.Click += delegate
            {
                LinearPatchColor.Visibility = ViewStates.Visible;
                LinearPatchTabs.Enabled = false;
            };

            TextCurrentColor.Text = "Current Patch Color: "+ TokketUserCur.PointsSymbolColor.Substring(0, 1).ToUpper() + TokketUserCur.PointsSymbolColor.Substring(1, TokketUserCur.PointsSymbolColor.Length - 1);
            ListPatchesColor = PointsSymbolsHelper.PatchesColors();
            BtnChangeColorCmd.Click += async(sender, e) =>
            {
                int position = 0;
                if (BtnChangeColorCmd == null)
                {
                    var dialog = new Android.App.AlertDialog.Builder(this);
                    var alertDialog = dialog.Create();
                    alertDialog.SetTitle("");
                    alertDialog.SetMessage("No color selected!");
                    alertDialog.SetButton((int)(DialogButtonType.Positive), Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                    alertDialog.Show();
                    alertDialog.SetCanceledOnTouchOutside(false);
                }
                else
                {
                    try { position = (int)BtnChangeColorCmd.Tag; } catch { position = int.Parse((string)BtnChangeColorCmd.Tag); }
                    string color = ListPatchesColor[position].Name.ToLower();
                    var result = await BadgeService.Instance.UpdateUserPointsSymbolColorAsync(ListPatchesColor[position].Name.ToLower(), UserId);

                    if (result)
                    {
                        var dialog = new Android.App.AlertDialog.Builder(this);
                        var alertDialog = dialog.Create();
                        alertDialog.SetTitle("");
                        alertDialog.SetMessage("Color is changed successfully!");
                        alertDialog.SetButton((int)(DialogButtonType.Positive), Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => 
                        {
                            TokketUserCur.PointsSymbolColor = color;
                            Settings.TokketUser = JsonConvert.SerializeObject(TokketUserCur);
                            LinearPatchTabs.Enabled = true;
                            LinearPatchColor.Visibility = ViewStates.Gone;
                            if (Settings.ActivityInt == (int)ActivityType.ProfileActivity)
                            {
                                ProfileUserActivity.Instance.ShowCurrentRank();
                            }
                            else if (Settings.ActivityInt == (int)ActivityType.ProfileTabActivity)
                            {
                                profile_fragment.Instance.ShowCurrentRank();
                            }
                            this.Finish();
                        });
                        alertDialog.Show();
                        alertDialog.SetCanceledOnTouchOutside(false);
                    }
                }
            };

            LoadData();
        }
        private void LoadData()
        {
            RecyclerColorPatches.SetLayoutManager(new GridLayoutManager(this, 1));
            var adapter = new PatchesAdapter(ListPatchesColor, PatchesTab.PatchColor, null);
            RecyclerColorPatches.SetAdapter(adapter);
        }
        void setupViewPager(ViewPager viewPager)
        {
            List<XFragment> fragments = new List<XFragment>();
            List<string> fragmentTitles = new List<string>();

            fragments.Add(new patches_fragment("My Patches"));
            fragments.Add(new patches_fragment("Level Table"));

            fragmentTitles.Add("My Patches");
            fragmentTitles.Add("Level Table");

            var adapter = new AdapterFragmentX(SupportFragmentManager, fragments, fragmentTitles);
            viewPager.Adapter = adapter;
            ViewPagerPatches.Adapter.NotifyDataSetChanged();
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (LinearPatchColor.Visibility == ViewStates.Visible)
                    {
                        LinearPatchTabs.Enabled = true;
                        LinearPatchColor.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        Finish();
                    }
                    
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        public override void OnBackPressed()
        {
            if (LinearPatchColor.Visibility == ViewStates.Visible)
            {
                LinearPatchTabs.Enabled = true;
                LinearPatchColor.Visibility = ViewStates.Gone;
            }
            else
            {
                base.OnBackPressed();
            }
        }
        public Button BtnChangePatchColor => FindViewById<Button>(Resource.Id.BtnChangePatchColor);
        public TabLayout TabPatches => FindViewById<TabLayout>(Resource.Id.tabLayoutPatches);
        public ViewPager ViewPagerPatches => FindViewById<ViewPager>(Resource.Id.viewpagerPatches);
        public LinearLayout LinearPatchTabs => FindViewById<LinearLayout>(Resource.Id.LinearPatchTabs);
        public LinearLayout LinearPatchColor => FindViewById<LinearLayout>(Resource.Id.LinearPatchColor);
        public TextView TextCurrentColor => FindViewById<TextView>(Resource.Id.TextCurrentColor);
        public RecyclerView RecyclerColorPatches => FindViewById<RecyclerView>(Resource.Id.RecyclerColorPatches);
        public Button BtnChangeColorCmd => FindViewById<Button>(Resource.Id.BtnChangeColorCmd);
    }
}