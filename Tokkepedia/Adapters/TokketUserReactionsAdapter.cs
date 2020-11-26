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
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Tokkepedia.Shared.Models;
using Tokkepedia.ViewHolders;

namespace Tokkepedia.Adapters
{
    class TokketUserReactionsAdapter : RecyclerView.Adapter
    {

        #region Members/Properties
        public event EventHandler<int> ItemClick;
        View itemView;
        List<TokketUserReaction> Users;
        public override int ItemCount => Users.Count;
        #endregion

        #region Constructor
        public TokketUserReactionsAdapter(List<TokketUserReaction> _users)
        {
            Users = _users;
        }
        public void UpdateItems(List<TokketUserReaction> listUpdate)
        {
            Users.AddRange(listUpdate);
            NotifyDataSetChanged();
        }
        #endregion

        #region Override Events/Methods/Delegates
        UserReactionsViewHolder vh;
        int selectedPosition = -1;
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            vh = holder as UserReactionsViewHolder;
            Android.Content.Res.Resources res = Application.Context.Resources;

            Glide.With(itemView).Load(Users[position].UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(vh.UserPhoto);
            vh.Username.Text = Users[position].DisplayName;

            if (Users[position].Kind == "gema")
            {
                Glide.With(itemView).AsBitmap().Load(Resource.Drawable.gem2_Green).Into(vh.UserReactions);
            }
            else if (Users[position].Kind == "gemb")
            {
                Glide.With(itemView).AsBitmap().Load(Resource.Drawable.gem4_Yellow).Into(vh.UserReactions);
            }
            else if (Users[position].Kind == "gemc")
            {
                Glide.With(itemView).AsBitmap().Load(Resource.Drawable.gem4_Red).Into(vh.UserReactions);
            }
            else if (Users[position].Kind == "accurate")
            {
                Glide.With(itemView).AsBitmap().Load(Resource.Drawable.check_black_48dp).Into(vh.UserReactions);
            }
            else if (Users[position].Kind == "inaccurate")
            {
                Glide.With(itemView).AsBitmap().Load(Resource.Drawable.clear_black_48dp).Into(vh.UserReactions);
            }
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.reactionvalues_users_row, parent, false);

            vh = new UserReactionsViewHolder(itemView, OnClick);

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