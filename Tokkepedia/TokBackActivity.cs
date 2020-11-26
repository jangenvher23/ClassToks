using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Newtonsoft.Json;
using Tokkepedia.Shared.Models;

namespace Tokkepedia
{
    [Activity(Label = "", Theme = "@style/AppTheme")]
    public class TokBackActivity : BaseActivity
    {
        TokModel tokModel; Spinner spinner_tokbackNumBlocks;
        LinearLayout linear_tokbackNumberBlocks, linear_tokbackDetail;
        private bool Showingback; TextView tokgroup; string stringblocks = "";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.tokback_page);

            var tokback_toolbar = this.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.tokback_toolbar);

            SetSupportActionBar(tokback_toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            tokModel = JsonConvert.DeserializeObject<TokModel>(Intent.GetStringExtra("tokModel"));

            linear_tokbackNumberBlocks = FindViewById<LinearLayout>(Resource.Id.linear_tokbackNumberBlocks);
            linear_tokbackDetail = FindViewById<LinearLayout>(Resource.Id.linear_tokbackDetail);
            var tokcategory = FindViewById<TextView>(Resource.Id.lblTokBackTokCategory);
            tokgroup = FindViewById<TextView>(Resource.Id.lblTokBackTokGroup);
            var toktype = FindViewById<TextView>(Resource.Id.lblTokBackTokType);
            var lblTokBackPrimaryField = FindViewById<TextView>(Resource.Id.lblTokBackPrimaryField);
            spinner_tokbackNumBlocks = FindViewById<Spinner>(Resource.Id.spinner_tokblocks);
            
            tokcategory.Text = tokModel.Category;
            tokgroup.Text = tokModel.TokGroup;
            toktype.Text = tokModel.TokType;

            if (tokgroup.Text.ToLower() == "quote")
            {
                lblTokBackPrimaryField.Text = tokModel.SecondaryFieldText;
                stringblocks = tokModel.PrimaryFieldText;
            }
            else
            {
                lblTokBackPrimaryField.Text = tokModel.PrimaryFieldText;
                stringblocks = tokModel.SecondaryFieldText;
            }
            

            if (tokModel.IsDetailBased)
            {
                linear_tokbackNumberBlocks.Visibility = ViewStates.Gone;

                linear_tokbackDetail.RemoveAllViews();

                for (int i = 0; i < tokModel.Details.Length; i++)
                {
                    if (!string.IsNullOrEmpty(tokModel.Details[i]))
                    {
                        View view = LayoutInflater.Inflate(Resource.Layout.tokback_detailview, null);
                        TextView lblTokBackCardBack = view.FindViewById<TextView>(Resource.Id.lblTokBackCardBack);
                        lblTokBackCardBack.Text = tokModel.Details[i];
                        view.Tag = i;
                        linear_tokbackDetail.AddView(view);

                        //Show the back for the first row
                        if (i == 0)
                        {
                            OnTapViewFlipperTokBack(view);
                        }
                    }
                }

            }
            else
            {
                spinner_tokbackNumBlocks.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(txtTokBackNumberBlock_ItemSelected);
                string[] arrayNumBlocks = new string[] { "3", "4", "5", "6", "7" };
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, arrayNumBlocks);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinner_tokbackNumBlocks.Adapter = adapter;
            }
        }
        private void txtTokBackNumberBlock_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            int selectedvalue = Convert.ToInt32(spinner.GetItemAtPosition(e.Position).ToString());

            if (!string.IsNullOrEmpty(stringblocks))
            {
                string textShowed = "";
                string primaryText = stringblocks;
                string[] arrDetail = stringblocks.Split(" ");
                TextView lblTokBackCardBack;
                if (arrDetail.Length >= 3 && Convert.ToInt32(selectedvalue) <= arrDetail.Length)
                {
                    linear_tokbackDetail.RemoveAllViews();

                    for (int i = 0; i < selectedvalue; i++)
                    {
                        View view = LayoutInflater.Inflate(Resource.Layout.tokback_detailview, null);
                        lblTokBackCardBack = view.FindViewById<TextView>(Resource.Id.lblTokBackCardBack);

                        if (i == 6)
                        {
                            lblTokBackCardBack.Text = primaryText.Replace(textShowed,"").Trim();
                        }
                        else
                        {
                            lblTokBackCardBack.Text = arrDetail[i];
                            textShowed += arrDetail[i] + " ";
                        }

                        view.Tag = i;
                        linear_tokbackDetail.AddView(view);

                        //Show the back for the first row
                        if (i==0)
                        {
                            OnTapViewFlipperTokBack(view);
                        }
                    }
                }
                else
                {
                    var objBuilder = new Android.App.AlertDialog.Builder(this);
                    objBuilder.SetTitle("");
                    objBuilder.SetIcon(Resource.Drawable.tokback_icon);
                    objBuilder.SetMessage("Not compatible with " + spinner.GetItemAtPosition(e.Position) + " blocks!");
                    objBuilder.SetCancelable(false);

                    Android.App.AlertDialog objDialog = objBuilder.Create();
                    objDialog.SetButton((int)(DialogButtonType.Positive), "OK", (s, ev) => { });
                    objDialog.Show();
                }
            }
        }

        [Java.Interop.Export("OnTapViewFlipperTokBack")]
        public void OnTapViewFlipperTokBack(View v)
        {
            var flipper = v.FindViewById<ViewFlipper>(Resource.Id.viewFlipper_tokback);
            if (Showingback)
            { //Front
                // Use custom animations
                flipper.SetOutAnimation(this, Resource.Animation.card_flip_bottom_out);
                flipper.SetInAnimation(this, Resource.Animation.card_flip_bottom_in);
                flipper.ShowPrevious();
                Showingback = false;
            }
            else
            { //Back
                // Use custom animations
                flipper.SetOutAnimation(this, Resource.Animation.card_flip_top_out);
                flipper.SetInAnimation(this, Resource.Animation.card_flip_top_in);
                flipper.ShowNext();
            }
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}