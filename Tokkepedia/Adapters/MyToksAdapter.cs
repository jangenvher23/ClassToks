using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Tokkepedia.ViewHolders;
using Tokket.Tokkepedia;
using Tokkepedia.Shared.Extensions;
using Newtonsoft.Json;
using Tokkepedia.ViewModels;
using Tokkepedia.Shared.Models;
using AndroidX.RecyclerView.Widget;
using Color = Android.Graphics.Color;

namespace Tokkepedia.Adapters
{
    public class MyToksAdapter : RecyclerView.Adapter, View.IOnTouchListener
    {
        public event EventHandler<int> ItemClick;
#if (_TOKKEPEDIA)
        public List<TokModel> items;
#endif
#if (_CLASSTOKS)
        public List<ClassTokModel> items;
#endif
        int cnttoksselected = 0;
        List<string> Colors = new List<string>() {
               "#d32f2f", "#C2185B", "#7B1FA2", "#512DA8",
               "#303F9F", "#1976D2", "#0288D1", "#0097A7",
               "#00796B", "#388E3C", "#689F38", "#AFB42B",
               "#FBC02D", "#FFA000", "#F57C00", "#E64A19"
               };
        List<string> randomcolors = new List<string>();
        public override int ItemCount => items.Count();
        View itemView;
#region Constructor
        public MyToksAdapter(List<TokModel> _items, List<ClassTokModel> _itemClass)
        {
#if (_TOKKEPEDIA)
        items = _items;
#endif
#if (_CLASSTOKS)
            items = _itemClass;
#endif
        }
#endregion

#region Override Events/Methods/Delegates
        MySetsViewHolder vh;
        public MySetsViewModel MySetsVm => App.Locator.MySetsPageVM;
        AssetManager assetManager = Application.Context.Assets;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            vh = holder as MySetsViewHolder;

            //Set LinearColor
            int ndx = position % Colors.Count;
            if (ndx == 0 || randomcolors.Count == 0) randomcolors = Colors.Shuffle().ToList();
            vh.linearMySetsColor.SetBackgroundColor(Color.ParseColor(randomcolors[ndx]));

            MySetsVm.lblMySetPopUp = vh.lblMySetPopUp;
            vh.txtSetsTokUpper.Text = items[position].PrimaryFieldText;

            vh.txtSetsTokBottom.Text = items[position].TokGroup + " - " + items[position].TokType + " • " + (Convert.ToDateTime(items[position].DateCreated)).ToString("MM/dd/yyyy");

            vh.lblMySetPopUp.Tag = position;
            vh.ItemView.Tag = position;
            vh.ItemView.SetOnTouchListener(this);
        }

        public void OnItemRowClick(object sender, int position)
        {
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.mytoksets_row, parent, false);

            vh = new MySetsViewHolder(itemView, OnClick);
            return vh;
        }
        public override int GetItemViewType(int position)
        {
            return position;

            //This was added due to the reason that when clicked, there will be 2 rows that will highlight
            //For Reference, this one have similar issue with the link below
            //https://stackoverflow.com/questions/32065267/recyclerview-changing-items-during-scroll
        }
#endregion

#region Custom Events/Delegates
        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            if (e.Action == MotionEventActions.Up)
            {
                int position = (int)v.Tag;
                if (string.IsNullOrEmpty(v.ContentDescription))
                {
                    v.ContentDescription = "selected";
                    v.SetBackgroundColor(Color.LightBlue);
                    cnttoksselected += 1;
                    MySetsVm.TokIdsList.Add(items[position].Id);
                    MySetsVm.TokPKsList.Add(items[position].PartitionKey);
                }
                else
                {
                    v.ContentDescription = "";
                    v.SetBackgroundColor(Color.Transparent);
                    cnttoksselected -= 1;
                    MySetsVm.TokIdsList.Remove(items[position].Id);
                    MySetsVm.TokPKsList.Remove(items[position].PartitionKey);
                }

#if (_TOKKEPEDIA)
                if (cnttoksselected == 1)
                {
                    MySetsActivity.Instance.txtTotalToksSelected.Text = cnttoksselected + " Tok Selected";
                }
                else if (cnttoksselected > 1)
                {
                    MySetsActivity.Instance.txtTotalToksSelected.Text = cnttoksselected + " Toks Selected";
                }
                else
                {
                    MySetsActivity.Instance.txtTotalToksSelected.Text = "";
                }
#endif
#if (_CLASSTOKS)
                if (cnttoksselected == 1)
                {
                    MyClassSetsActivity.Instance.txtTotalToksSelected.Text = cnttoksselected + " Tok Selected";
                }
                else if (cnttoksselected > 1)
                {
                    MyClassSetsActivity.Instance.txtTotalToksSelected.Text = cnttoksselected + " Toks Selected";
                }
                else
                {
                    MyClassSetsActivity.Instance.txtTotalToksSelected.Text = "";
                }
#endif
            }
            return true;
        }
#endregion
    }
}