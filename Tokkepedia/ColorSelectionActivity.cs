using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Tokkepedia.Shared.Helpers;
using GalaSoft.MvvmLight.Helpers;
using Tokkepedia.Shared.Services;
using GridLayoutManager = Android.Support.V7.Widget.GridLayoutManager;
using RecyclerView = Android.Support.V7.Widget.RecyclerView;
using Result = Android.App.Result;
using Tokkepedia.Model;
using Tokkepedia.Shared.Models;
using Android.Views;

namespace Tokkepedia
{
    [Activity(Label = "Color Selection", Theme = "@style/Theme.AppCompat.Light.Dialog.NoTitle")]
    public class ColorSelectionActivity : BaseActivity
    {
        List<string> Colors32 = new List<string>() {
        "#e57373","#f06292","#ba68c8","#9575cd",
        "#d32f2f", "#C2185B", "#7B1FA2", "#512DA8",
        "#7986cb", "#64b5f6", "#4fc3f7", "#4dd0e1",
        "#303F9F", "#1976D2", "#0288D1", "#0097A7",
        "#4db6ac", "#81c784", "#aed581", "#dce775",
        "#00796B", "#388E3C", "#689F38", "#AFB42B",
        "#fff176", "#ffd54f", "#ffb74d", "#ff8a65",
        "#FBC02D", "#FFA000", "#F57C00", "#E64A19" };
        private ObservableRecyclerAdapter<ColorModel, CachingViewHolder> adapterColors;
        ObservableCollection<ColorModel> colorsCollection;
        string colorHex = "", keyvalue = "", className = "";
        DefaultColor defaultColor;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_color_selection);

            defaultColor = new DefaultColor();
            className = Intent.GetStringExtra("className");
            colorHex = Intent.GetStringExtra("color");
            keyvalue = Intent.GetStringExtra("keyvalue");

            colorsCollection = new ObservableCollection<ColorModel>();
            foreach (var col in Colors32)
            {
                ColorModel color = new ColorModel();
                color.color = col;
                colorsCollection.Add(color);
            }

            recyclerColors.SetLayoutManager(new GridLayoutManager(this, 4));
            adapterColors = colorsCollection.GetRecyclerAdapter(BindColorsViewHolder, Resource.Layout.color_selection_row);
            recyclerColors.SetAdapter(adapterColors);

            btnClose.Click += delegate
            {
                this.Finish();
            };

            btnSelect.Click += async(s, e) =>
            {
                if (btnSelect.ContentDescription.Trim() != "")
                {
                    Intent intent = new Intent();
                    intent.PutExtra("color", btnSelect.ContentDescription);
                    SetResult(Result.Ok, intent);
                    this.Finish();
                }
            };

            btnRemovecolor.Click += delegate
            {
                Intent intent = new Intent();
                intent.PutExtra("color", "#FFFFFF");
                SetResult(Result.Ok, intent);
                this.Finish();
            };

            chkSetDefault.CheckedChange += async(s, e) =>
            {
                if (string.IsNullOrEmpty(btnSelect.ContentDescription))
                {
                    defaultColor = new DefaultColor();
                    btnSelect.ContentDescription = defaultColor.ColorHex;
                }

                if (chkSetDefault.Checked)
                {
                    txtProgressText.Text = "Checking default color for " + className + "...";
                    await SetDefaultColor();
                }
            };
        }
        private async Task SetDefaultColor()
        {
            btnSelect.Enabled = false;
            string colorHex = btnSelect.ContentDescription;
            string userId = Settings.GetUserModel().UserId;

            linearProgress.Visibility = ViewStates.Visible;
            this.Window.AddFlags(WindowManagerFlags.NotTouchable);

            defaultColor = await AccessoriesService.Instance.DefaultColorAsync(colorHex: colorHex, userId: userId, keyvalue, method: "post");
            if (defaultColor == null)
            {
                defaultColor = await AccessoriesService.Instance.DefaultColorAsync(colorHex: colorHex, userId: userId, keyvalue, method: "post");
            }
            else if (defaultColor.ColorHex != colorHex)
            {
                defaultColor = await AccessoriesService.Instance.DefaultColorAsync(colorHex: colorHex, userId: userId, keyvalue, method: "post");
            }

            btnSelect.ContentDescription = defaultColor.ColorHex;
            btnSelect.Enabled = true;

            linearProgress.Visibility = ViewStates.Gone;
            this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
        }

        private void BindColorsViewHolder(CachingViewHolder holder, ColorModel color, int position)
        {
            var linearView = holder.FindCachedViewById<LinearLayout>(Resource.Id.linearView);
            var txtColor = holder.FindCachedViewById<TextView>(Resource.Id.txtColor);
            txtColor.SetBackgroundColor(Color.ParseColor(color.color));
            linearView.SetBackgroundColor(Color.ParseColor(color.color));

            txtColor.Click += delegate
            {
                foreach(var col in colorsCollection)
                {
                    col.isSelected = false;
                };

                color.isSelected = !color.isSelected;

                defaultColor = new DefaultColor();
                btnSelect.ContentDescription = defaultColor.ColorHex;

                recyclerColors.SetAdapter(adapterColors);
            };

            if (color.isSelected)
            {
                btnSelect.ContentDescription = color.color;
                linearView.SetBackgroundColor(Color.White);
            }
        }
        public RecyclerView recyclerColors => FindViewById<RecyclerView>(Resource.Id.recyclerColors);
        public Button btnClose => FindViewById<Button>(Resource.Id.btnClose);
        public Button btnSelect => FindViewById<Button>(Resource.Id.btnSelect);
        public Button btnRemovecolor => FindViewById<Button>(Resource.Id.btnRemoveColor);
        public CheckBox chkSetDefault => FindViewById<CheckBox>(Resource.Id.chkSetDefault);
        public LinearLayout linearProgress => FindViewById<LinearLayout>(Resource.Id.linearProgress);
        public TextView txtProgressText => FindViewById<TextView>(Resource.Id.txtProgressText);
    }
}