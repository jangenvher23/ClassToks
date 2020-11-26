using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
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
using AndroidX.RecyclerView.Widget;
using Color = Android.Graphics.Color;

namespace Tokkepedia.Adapters
{
    public class InviteUserAdapter : RecyclerView.Adapter
    {

        public event EventHandler<int> ItemClick;
        public ObservableCollection<TokketUser> UsersCollection;
        ObservableCollection<ClassGroupRequestModel> UserRequestsCollection;
        ClassGroupModel model;
        public override int ItemCount => UsersCollection.Count();
        View itemView;

        #region Constructor
        public InviteUserAdapter(ObservableCollection<TokketUser> _items, ObservableCollection<ClassGroupRequestModel> _UserRequestsCollection, ClassGroupModel _model)
        {
            UsersCollection = _items;
            model = _model;
            UserRequestsCollection = _UserRequestsCollection;
        }
        #endregion

        #region Override Events/Methods/Delegates
        InviteUsersViewHolder vh;
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            vh = holder as InviteUsersViewHolder;

            vh.BtnInvite.Tag = position;
            vh.Username.Text = UsersCollection[position].DisplayName;
            Glide.With(itemView).Load(UsersCollection[position].UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(vh.ImgUserPhoto);

            //Check if user have pending invites sent
            var resultRequest = UserRequestsCollection.FirstOrDefault(c => c.ReceiverId == UsersCollection[position].Id);
            if (resultRequest != null) //If Edit
            {
                vh.BtnInvite.Text = "Cancel";
                vh.BtnInvite.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Color.ParseColor("#dc3545"));
            }
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.inviteusers_row, parent, false);

            vh = new InviteUsersViewHolder(itemView, OnClick);
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