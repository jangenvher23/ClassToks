using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using GalaSoft.MvvmLight.Helpers;
using Newtonsoft.Json;
using Tokkepedia.Adapters;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using Tokkepedia.Helpers;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;

namespace Tokkepedia
{
#if (_TOKKEPEDIA)
    [Activity(Label = "Badge", Theme = "@style/CustomAppTheme")]
#endif

#if (_CLASSTOKS)
    [Activity(Label = "Badge", Theme = "@style/CustomAppThemeBlue")]
#endif

    public class BadgesActivity : BaseActivity
    {
        internal static BadgesActivity Instance { get; private set; }
        public ObservableCollection<BadgeOwned> BadgeCollection;
        string UserId; bool resultbool;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.badgespage);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            Instance = this;

            UserId = Settings.GetTokketUser().Id;

            string[] colors = new string[] { "Black","Blue","Green","Orange","Pink"};
            var spincolorAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, colors);
            spincolorAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            SpinnerColors.Adapter = null;
            SpinnerColors.Adapter = spincolorAdapter;

            RecyclerBadgePage.SetLayoutManager(new GridLayoutManager(this, 3));
            BadgeCollection = new ObservableCollection<BadgeOwned>();
            BadgeCollection.Clear();

            this.RunOnUiThread(async () => await Initialize());

            SelectCommand.Click += async(sender,e)=>
            {
                int selectedPosition = (int)SelectCommand.Tag;

                BadgeOwned BadgeSelected = BadgeCollection[selectedPosition];

                LinearProgress.Visibility = ViewStates.Visible;
                this.Window.AddFlags(WindowManagerFlags.NotTouchable);

                resultbool = await BadgeService.Instance.SelectBadgeAsync(BadgeSelected.Id);

                this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                LinearProgress.Visibility = ViewStates.Gone;

                string message = "";
                if (resultbool)
                {
                    message = "Badge selected as profile picture successfully!";
                }
                else
                {
                    message = "Failed to select.";
                }

                var dialog = new Android.App.AlertDialog.Builder(this);
                var alertDialog = dialog.Create();
                alertDialog.SetTitle("");
                alertDialog.SetIcon(Resource.Drawable.alert_icon_blue);
                alertDialog.SetMessage(message);
                alertDialog.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) =>
                {
                    if (resultbool)
                    {
                        var badgeConvert = JsonConvert.SerializeObject(BadgeSelected);
                        Intent = new Intent();
                        Intent.PutExtra("Badge", badgeConvert);
                        SetResult(Android.App.Result.Ok, Intent);
                        Finish();
                    }
                });
                alertDialog.Show();
                alertDialog.SetCanceledOnTouchOutside(false);
            };

            ChangeColorCommand.Click += (sender, e) =>
            {
                if (SelectCommand.Tag == null)
                {
                    var dialog = new Android.App.AlertDialog.Builder(this);
                    var alertDialog = dialog.Create();
                    alertDialog.SetTitle("");
                    alertDialog.SetMessage("Select Badge first!");
                    alertDialog.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => { });
                    alertDialog.Show();
                    alertDialog.SetCanceledOnTouchOutside(false);
                }
                else
                {
                    RelativeBadgeParent.Enabled = false;
                    LinearBadgeColor.Visibility = ViewStates.Visible;
                    this.Title = "Select color:";
                }
            };

            ColorUpdateCmd.Click += async(sender, e) =>
            {
                int selectedPosition = (int)SelectCommand.Tag;

                BadgeOwned BadgeSelected = BadgeCollection[selectedPosition];

                LinearProgress.Visibility = ViewStates.Visible;
                this.Window.AddFlags(WindowManagerFlags.NotTouchable);

                resultbool = await BadgeService.Instance.UpdateUserBadgeColor(BadgeSelected.Id,UserId, SpinnerColors.GetItemAtPosition(SpinnerColors.FirstVisiblePosition).ToString().ToLower());

                this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                LinearProgress.Visibility = ViewStates.Gone;

                string message = "";
                if (resultbool)
                {
                    message = "Color changed successfully!";
                }
                else
                {
                    message = "Failed to change.";
                }

                var dialog = new Android.App.AlertDialog.Builder(this);
                var alertDialog = dialog.Create();
                alertDialog.SetTitle("");
                alertDialog.SetMessage(message);
                alertDialog.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), async(d, fv) =>
                {
                    if (resultbool)
                    {
                        BadgeCollection = new ObservableCollection<BadgeOwned>();
                        LinearBadgeColor.Visibility = ViewStates.Gone;
                        await Initialize();
                    }
                });
                alertDialog.Show();
                alertDialog.SetCanceledOnTouchOutside(false);
            };
        }
        private async Task Initialize()
        {
            var resultAvatars = await BadgeService.Instance.GetUserBadgesAsync(UserId);
            var resultList = resultAvatars.Results.ToList();
            foreach (var badge in resultList)
            {
                BadgeCollection.Add(badge);
            }

            var adapterTokMoji = new BadgesAdapter(BadgeCollection);
            RecyclerBadgePage.SetAdapter(adapterTokMoji);
        }
        public override void OnBackPressed()
        {
            if (LinearBadgeColor.Visibility == ViewStates.Visible)
            {
                RelativeBadgeParent.Enabled = true;

                LinearBadgeColor.Visibility = ViewStates.Gone;
                this.Title = "Badge";
            }
            else
            {
                base.OnBackPressed();
            }
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (LinearBadgeColor.Visibility == ViewStates.Visible)
                    {
                        RelativeBadgeParent.Enabled = true;

                        LinearBadgeColor.Visibility = ViewStates.Gone;
                        this.Title = "Badge";
                    }
                    else
                    {
                        Finish();
                    }
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        public RecyclerView RecyclerBadgePage => FindViewById<RecyclerView>(Resource.Id.RecyclerBadgePage);
        public Button ChangeColorCommand => FindViewById<Button>(Resource.Id.btnChangeBadgeColor);
        public Button SelectCommand => FindViewById<Button>(Resource.Id.btnSelectBadge); 
        public LinearLayout LinearProgress => FindViewById<LinearLayout>(Resource.Id.linear_BadgeProgress);
        public TextView TextProgressStatus => FindViewById<TextView>(Resource.Id.TextProgressStatus);
        public LinearLayout LinearBadgeColor => FindViewById<LinearLayout>(Resource.Id.LinearBadgeColor);
        public Spinner SpinnerColors => FindViewById<Spinner>(Resource.Id.SpinnerColors);
        public Button ColorUpdateCmd => FindViewById<Button>(Resource.Id.btnChangeCommand);
        public RelativeLayout RelativeBadgeParent => FindViewById<RelativeLayout>(Resource.Id.RelativeBadgeParent);
    }
}