using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Tokkepedia.Shared.Models;
using Tokkepedia.ViewHolders;
using Tokket.Tokkepedia;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Services;
using Android.Graphics;
using Android.Text;
using Tokkepedia.Helpers;
using Color = Android.Graphics.Color;

namespace Tokkepedia.Adapters
{
    public class ProfileTitleAdapter : RecyclerView.Adapter
    {
        int selectedPosition = -1;
        View holderView;
        List<TokketTitle> TokketTitleList;
        public event EventHandler<int> ItemClick;
        public override int ItemCount => TokketTitleList.Count();
        View itemView;

        #region Constructor
        public ProfileTitleAdapter(List<TokketTitle> _TokketTitleList)
        {
            TokketTitleList = _TokketTitleList;
        }
        #endregion

        #region Override Events/Methods/Delegates
        ProfileTitleViewHolder vh;
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            vh = holder as ProfileTitleViewHolder;

            vh.TextTitle.Text = TokketTitleList[position].TitleDisplay;

            holder.ItemView.Click += (sender, e) =>
            {
                if (selectedPosition != -1)
                {
                    holderView.SetBackgroundColor(Color.Transparent);
                }
                holder.ItemView.SetBackgroundColor(Color.LightBlue);

                ProfileTitleActivity.Instance.BtnSelect.Tag = position;
                ProfileTitleActivity.Instance.TextTitleSelected.ContentDescription = TokketTitleList[position].Id;
                ProfileTitleActivity.Instance.TextTitleSelected.Text = TokketTitleList[position].TitleDisplay;

                selectedPosition = position;
                holderView = holder.ItemView;
            };
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.profiletitle_row, parent, false);

            vh = new ProfileTitleViewHolder(itemView, OnClick);
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
        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }
    }
}