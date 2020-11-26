using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Tokkepedia.ViewHolders
{
    public class StickerViewHolder : RecyclerView.ViewHolder
    {
        public ImageView StickerImage { get; private set; }
        public TextView StickerTitle { get; private set; }
        public TextView NumCoins { get; private set; }

        public StickerViewHolder(View itemView, Action<int> listener)
            : base(itemView)
        {
            // Locate and cache view references:
            //Tok Image
            StickerImage = itemView.FindViewById<ImageView>(Resource.Id.imgaddtok_stickerimage);
            StickerTitle = itemView.FindViewById<TextView>(Resource.Id.lbladdtok_stickertitle);
            NumCoins = itemView.FindViewById<TextView>(Resource.Id.lbl_addtoknumcoins);


            // Detect user clicks on the item view and report which item
            // was clicked (by layout position) to the listener:
            itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }
}