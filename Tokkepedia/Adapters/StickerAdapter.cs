using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Tokkepedia.ViewHolders;

namespace Tokkepedia.Adapters
{
    public class StickerAdapter : RecyclerView.Adapter
    {
        #region Members/Properties
        public event EventHandler<int> ItemClick;
        View itemView;
        List<Tokket.Tokkepedia.Sticker> items;
        public override int ItemCount => items.Count;
        #endregion

        #region Constructor
        public StickerAdapter(List<Tokket.Tokkepedia.Sticker> _items)
        {
            items = _items;
        }
        #endregion

        #region Override Events/Methods/Delegates
        StickerViewHolder vh;
        int selectedPosition = -1;
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            vh = holder as StickerViewHolder;
            Android.Content.Res.Resources res = Application.Context.Resources;

            Glide.With(itemView).Load(items[position].Image).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).FitCenter()).Into(vh.StickerImage);
            vh.StickerTitle.Text = items[position].Name;
            vh.NumCoins.Text = items[position].PriceCoins.ToString();

            if (selectedPosition == position)
            {
                vh.ItemView.SetBackgroundColor(Color.LightBlue);
            }
            else if (selectedPosition != position)
            {
                vh.ItemView.SetBackgroundColor(Color.Transparent);
            }

            vh.ItemView.Click += (sender, e) =>
            {
                selectedPosition = position;
                NotifyDataSetChanged();
            };
        }

        public void OnGridBackgroundClick(object sender, int position)
        {

        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.modal_addsticker_row, parent, false);

            vh = new StickerViewHolder(itemView, OnClick);

            return vh;
        }
        #endregion

        #region Custom Events/Delegates
        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }
        #endregion
    }
}