using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using GalaSoft.MvvmLight.Helpers;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using Tokkepedia.Adapters;
using Tokkepedia.Fragments;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokkepedia.ViewModels;
using Tokket.Tokkepedia;
using Color = Android.Graphics.Color;
using XFragment = AndroidX.Fragment.App.Fragment;
using XFragmentManager = AndroidX.Fragment.App.FragmentManager;

namespace Tokkepedia
{
    [Activity(Label = "Class Sets")]
    public class MyClassSetsActivity : BaseActivity
    {
        internal static MyClassSetsActivity Instance { get; private set; }
        string tabheader = "My Class Tok Sets";
        bool isAddToSet; string toktypeid = "";
        Set SetModel; ClassTokModel ClassTokMode;
        int uiOptions;
        public MySetsViewModel MySetsVm => App.Locator.MySetsPageVM;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.mysets_page);

            /*//====================================
            uiOptions = (int)Window.DecorView.SystemUiVisibility;

            uiOptions |= (int)SystemUiFlags.LowProfile;
            //uiOptions |= (int)SystemUiFlags.Fullscreen;
            uiOptions |= (int)SystemUiFlags.HideNavigation;
            uiOptions |= (int)SystemUiFlags.ImmersiveSticky;

            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            //====================================*/

            Instance = this;
            MySetsVm.Instance = Instance;

            toktypeid = Intent.GetStringExtra("TokTypeId").ToString();
            MySetsVm.TokTypeID = toktypeid;

            MySetsVm.LinearProgress = LinearProgress;
            MySetsVm.ProgressCircle = ProgressCircle;
            MySetsVm.ProgressText = ProgressText;

            isAddToSet = Intent.GetBooleanExtra("isAddToSet", true);
            MySetsVm.IsAddToksToSet = isAddToSet;

            BtnCancel.SetCommand("Click", MySetsVm.CancelSetCommand);
            //BtnAddClassSet.Click += delegate
            //{
            //    var nextActivity = new Intent(MainActivity.Instance, typeof(AddClassSetActivity));
            //    this.StartActivity(nextActivity);
            //};

            LinearToolbar.SetBackgroundColor(Color.ParseColor("#3498db"));

            setupViewPager(viewpager);
            tabLayout.SetupWithViewPager(viewpager);

            if (Settings.ActivityInt == Convert.ToInt16(ActivityType.MySetsView))
            {
                //If opened from MySetsViewActivity
                if (isAddToSet)
                {
                    BtnAddClassSet.Text = "Add to Set";
                    BtnAddClassSet.SetCommand("Click", MySetsVm.AddSetCommand);
                }
                else
                {
                    BtnAddClassSet.Text = "Remove from Set";
                    BtnAddClassSet.SetCommand("Click", MySetsVm.RemoveToksFromSetCommand);
                }

                tabheader = "Select a tok:";
                txtMySetsPageTitle.Visibility = ViewStates.Visible;

                SetModel = JsonConvert.DeserializeObject<Set>(Intent.GetStringExtra("classsetModel"));
                MySetsVm.SetModel = SetModel;
                txtMySetsPageTitle.Text = SetModel.Name;
            }
            else if (Settings.ActivityInt == Convert.ToInt16(ActivityType.LeftMenuSets))
            {
                //BtnAddClassSet.Text = "+ Add Class Set";
                BtnAddClassSet.SetCommand("Click", MySetsVm.AddSetCommand);
            }
            else if (Settings.ActivityInt == Convert.ToInt16(ActivityType.TokInfo))
            {
                ClassTokMode = JsonConvert.DeserializeObject<ClassTokModel>(Intent.GetStringExtra("classtokModel"));
                MySetsVm.tokList = ClassTokMode;
                txtMySetsPageTitle.Text = ClassTokMode.PrimaryFieldText;
                tabheader = "Select a class tok:";
                txtMySetsPageTitle.Visibility = ViewStates.Visible;

                BtnAddClassSet.Text = "Add to Class Set";
                BtnAddClassSet.SetCommand("Click", MySetsVm.AddSetCommand);
            }
        }
        void setupViewPager(ViewPager viewPager)
        {
            List<XFragment> fragments = new List<XFragment>();
            List<string> fragmentTitles = new List<string>();

            fragments.Add(new myclasstoksets_fragment(""));
            
            fragmentTitles.Add(tabheader);

            var adapter = new AdapterFragmentX(SupportFragmentManager, fragments, fragmentTitles);
            viewPager.Adapter = adapter;
            viewpager.Adapter.NotifyDataSetChanged();
        }
        [Java.Interop.Export("OnClickPopUpMenuST")]
        public void OnClickPopUpMenuST(View v)
        {
            var alertDiag = new Android.Support.V7.App.AlertDialog.Builder(Instance);
            Dialog diag;
            int position = 0;
            try { position = (int)v.Tag; } catch { position = int.Parse((string)v.Tag); }
            Android.Widget.PopupMenu menu = new Android.Widget.PopupMenu(this, v);

            // Call inflate directly on the menu:
            menu.Inflate(Resource.Menu.myclasssets_popmenu);

            // A menu item was clicked:
            menu.MenuItemClick += (s1, arg1) => {
                Intent nextActivity; string modelConvert;
                switch (arg1.Item.TitleFormatted.ToString().ToLower())
                {
                    case "edit":
                        nextActivity = new Intent(MainActivity.Instance, typeof(AddClassSetActivity));
                        modelConvert = JsonConvert.SerializeObject(myclasstoksets_fragment.Instance.ListClassTokSets[position]);
                        nextActivity.PutExtra("ClassTokSetsModel", modelConvert);
                        nextActivity.PutExtra("isSave",false);
                        this.StartActivity(nextActivity);
                        break;
                    case "delete":
                        alertDiag.SetTitle("Confirm");
                        alertDiag.SetMessage("Are you sure you want to continue?");
                        alertDiag.SetPositiveButton("Cancel", (senderAlert, args) => {
                            alertDiag.Dispose();
                        });
                        alertDiag.SetNegativeButton(Html.FromHtml("<font color='#dc3545'>Delete</font>", FromHtmlOptions.ModeLegacy), async (senderAlert, args) => {
                            ProgressCircle.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.LightBlue, Android.Graphics.PorterDuff.Mode.Multiply);
                            ProgressText.Text = "Deleting set...";
                            LinearProgress.Visibility = ViewStates.Visible;
                            Instance.Window.AddFlags(WindowManagerFlags.NotTouchable);

                            await ClassService.Instance.DeleteClassSetAsync(myclasstoksets_fragment.Instance.ListClassTokSets[position].Id, myclasstoksets_fragment.Instance.ListClassTokSets[position].PartitionKey);

                            LinearProgress.Visibility = ViewStates.Gone;
                            Instance.Window.ClearFlags(WindowManagerFlags.NotTouchable);

                            var dialogDelete = new Android.App.AlertDialog.Builder(Instance);
                            var alertDelete = dialogDelete.Create();
                            alertDelete.SetTitle("");
                            alertDelete.SetIcon(Resource.Drawable.alert_icon_blue);
                            alertDelete.SetMessage("Set deleted!");
                            alertDelete.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) =>
                            {
                                myclasstoksets_fragment.Instance.deleteItemClassSet(myclasstoksets_fragment.Instance.ListClassTokSets[position]);
                            });
                            alertDelete.Show();
                            alertDelete.SetCanceledOnTouchOutside(false);
                        });
                        diag = alertDiag.Create();
                        diag.Show();
                        break;
                    case "view class toks":
                        nextActivity = new Intent(MainActivity.Instance, typeof(MyClassSetsViewActivity));
                        modelConvert = JsonConvert.SerializeObject(myclasstoksets_fragment.Instance.ListClassTokSets[position]);
                        nextActivity.PutExtra("classsetModel", modelConvert);
                        this.StartActivity(nextActivity);
                        break;
                    case "play tok cards":
                        nextActivity = new Intent(this, typeof(TokCardsMiniGameActivity));
                        modelConvert = JsonConvert.SerializeObject(SetModel);
                        nextActivity.PutExtra("classsetModel", JsonConvert.SerializeObject(myclasstoksets_fragment.Instance.ListClassTokSets[position]));
                        this.StartActivity(nextActivity);
                        break;
                    case "play tok choice":
                        if (myclasstoksets_fragment.Instance.ListClassTokSets[position].TokIds.Count > 3)
                        {
                            alertDiag = new Android.Support.V7.App.AlertDialog.Builder(Instance);
                            alertDiag.SetTitle("Tok Choice");
                            alertDiag.SetMessage("Continue to Play Set?");
                            alertDiag.SetPositiveButton(Html.FromHtml("<font color='#dc3545'>Return</font>", FromHtmlOptions.ModeLegacy), (senderAlert, args) => {
                                alertDiag.Dispose();
                            });
                            alertDiag.SetNegativeButton(Html.FromHtml("<font color='#007bff'>Play</font>", FromHtmlOptions.ModeLegacy), (senderAlert, args) => {
                                nextActivity = new Intent(this, typeof(TokChoiceActivity));
                                modelConvert = JsonConvert.SerializeObject(SetModel);
                                nextActivity.PutExtra("classsetModel", JsonConvert.SerializeObject(myclasstoksets_fragment.Instance.ListClassTokSets[position]));
                                this.StartActivity(nextActivity);
                            });
                            diag = alertDiag.Create();
                            diag.Show();
                        }
                        else
                        {
                            var mssgDialog = new Android.App.AlertDialog.Builder(Instance);
                            var alertMssg = mssgDialog.Create();
                            alertMssg.SetTitle("");
                            alertMssg.SetIcon(Resource.Drawable.alert_icon_blue);
                            alertMssg.SetMessage("Tok Choice requires at least 4 toks in the set. Add more toks to play.");
                            alertMssg.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                            alertMssg.Show();
                        }

                        break;
                    case "play tok match":
                        nextActivity = new Intent(this, typeof(TokMatchActivity));
                        modelConvert = JsonConvert.SerializeObject(SetModel);
                        nextActivity.PutExtra("classsetModel", JsonConvert.SerializeObject(myclasstoksets_fragment.Instance.ListClassTokSets[position]));
                        nextActivity.PutExtra("isSet", true);
                        this.StartActivity(nextActivity);
                        break;
                }
            };

            // Menu was dismissed:
            menu.DismissEvent += (s2, arg2) => {
                //Console.WriteLine("menu dismissed");
            };

            menu.Show();
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
        public TextView txtMySetsPageTitle => this.FindViewById<TextView>(Resource.Id.txtMySetsPageTitle);
        public TextView BtnCancel => FindViewById<TextView>(Resource.Id.btnMySetsCancel);
        public TextView BtnAddClassSet => FindViewById<TextView>(Resource.Id.btnMySetsAdd);
        public LinearLayout LinearToolbar => this.FindViewById<LinearLayout>(Resource.Id.LinearSetsToolbar);
        public LinearLayout LinearProgress => FindViewById<LinearLayout>(Resource.Id.linear_mysetsprogress);
        public ProgressBar ProgressCircle => FindViewById<ProgressBar>(Resource.Id.progressbarMySets);
        public TextView ProgressText => FindViewById<TextView>(Resource.Id.progressBarTextMySets);
        public TextView MySetsPageTitle => this.FindViewById<TextView>(Resource.Id.txtMySetsPageTitle);
        public TabLayout tabLayout => this.FindViewById<TabLayout>(Resource.Id.tabMySets);
        public TextView TextNoSetsInfo => FindViewById<TextView>(Resource.Id.txtMySetsPageNoSets);
        public ViewPager viewpager => this.FindViewById<ViewPager>(Resource.Id.viewpagerMySets);
        public TextView txtTotalToksSelected => this.FindViewById<TextView>(Resource.Id.txtTotalToksSelected);
    }
}