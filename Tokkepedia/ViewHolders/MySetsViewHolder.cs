using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace Tokkepedia.ViewHolders
{
    public class MySetsViewHolder : RecyclerView.ViewHolder
    {
        
        public TextView txtSetsTokUpper { get; private set; }
        public TextView txtSetsTokBottom { get; private set; }
        public TextView lblMySetPopUp { get; private set; }
        public TextView txtClassDescription { get; private set; }
        public LinearLayout linearMySetsColor { get; private set; }
        public ImageView ImgMySetsRow { get; private set; }
        public MySetsViewHolder(View itemView, Action<int> listener)
                 : base(itemView)
        {
            // Locate and cache view references:                                     
            txtSetsTokUpper = itemView.FindViewById<TextView>(Resource.Id.txtSetsTokUpper);
            txtClassDescription = itemView.FindViewById<TextView>(Resource.Id.txtClassDescription);
            txtSetsTokBottom = itemView.FindViewById<TextView>(Resource.Id.txtSetsTokBottom);
            lblMySetPopUp = itemView.FindViewById<TextView>(Resource.Id.lblMySetPopUp);
            linearMySetsColor = itemView.FindViewById<LinearLayout>(Resource.Id.linearMySetsColor);
            ImgMySetsRow = itemView.FindViewById<ImageView>(Resource.Id.ImgMySetsRow);
            itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }
}