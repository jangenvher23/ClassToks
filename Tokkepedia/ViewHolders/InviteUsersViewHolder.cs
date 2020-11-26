using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace Tokkepedia.ViewHolders
{
    public class InviteUsersViewHolder : RecyclerView.ViewHolder
    {
        public ImageView ImgUserPhoto { get; private set; }
        public TextView Username { get; private set; }
        public Button BtnInvite { get; private set; }
        public ProgressBar ProgressCircle { get; private set; }
        public InviteUsersViewHolder(View itemView, Action<int> listener)
                 : base(itemView)
        {
            // Locate and cache view references:
            ImgUserPhoto = itemView.FindViewById<ImageView>(Resource.Id.ImgInviteUserphoto);
            Username = itemView.FindViewById<TextView>(Resource.Id.TextInviteUserName);
            BtnInvite = itemView.FindViewById<Button>(Resource.Id.BtnSubmitInvite);
            ProgressCircle = itemView.FindViewById<ProgressBar>(Resource.Id.ProgressInviteUsers);
            itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }
}