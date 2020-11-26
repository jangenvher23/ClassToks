using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using GalaSoft.MvvmLight.Helpers;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;   
using Android.Support.V7.Widget;
using Tokkepedia.Shared.Helpers;
using AndroidX.Fragment.App;

namespace Tokkepedia.Fragments
{
    public class users_fragment : AndroidX.Fragment.App.Fragment, View.IOnTouchListener
    {

        Android.Views.View v;
        public string filterText { get; set; } = "";
        private List<TokketUser> UserList;
        private RecyclerView RecyclerUsersContainer => v.FindViewById<RecyclerView>(Resource.Id.RecyclerContainer);
        private ProgressBar progressBar;
        private FragmentActivity myContext;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            UserList = new List<TokketUser>();
            v = inflater.Inflate(Resource.Layout.container, container, false);

            myContext = MainActivity.Instance;

            progressBar = v.FindViewById<ProgressBar>(Resource.Id.progressbar);
            progressBar.Visibility = ViewStates.Visible;

            RecyclerUsersContainer.SetLayoutManager(new LinearLayoutManager(Application.Context));

            ((Activity)Context).RunOnUiThread(async () => await InitializeUsers());

            Settings.ActivityInt = 0;

            return v;
        }

        private async Task InitializeUsers()
        {
            var resultUsers = await CommonService.Instance.SearchUsersAsync(filterText);
            if (resultUsers.Results.Count() == 0)
            {
                TextNothingFound.Text = "No users found.";
                TextNothingFound.Visibility = ViewStates.Visible;
            }
            else
            {
                TextNothingFound.Visibility = ViewStates.Gone;
            }
            UserList.AddRange(resultUsers.Results);
            SetUsersAdapter();
            progressBar.Visibility = ViewStates.Gone;
        }

        private void SetUsersAdapter()
        {
            var adapterUsers = UserList.GetRecyclerAdapter(BindUsersViewHolder, Resource.Layout.userrow);
            RecyclerUsersContainer.SetAdapter(adapterUsers);
        }

        private void BindUsersViewHolder(CachingViewHolder holder, TokketUser model, int position)
        {
            var ImgRequestUserphoto = holder.FindCachedViewById<ImageView>(Resource.Id.ImgRequestUserphoto);
            var TextUserRequestName = holder.FindCachedViewById<TextView>(Resource.Id.TextUserRequestName);

            Glide.With(myContext).Load(model.UserPhoto).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).Error(Resource.Drawable.Man3).FitCenter()).Into(ImgRequestUserphoto);
            TextUserRequestName.Text = model.DisplayName;
            ImgRequestUserphoto.ContentDescription = position.ToString();
            TextUserRequestName.ContentDescription = position.ToString();

            TextUserRequestName.SetOnTouchListener(this);
            ImgRequestUserphoto.SetOnTouchListener(this);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            int ndx = int.Parse((string)v.ContentDescription);
            if (e.Action == MotionEventActions.Up)
            {
                Intent nextActivity = new Intent(MainActivity.Instance, typeof(ProfileUserActivity));
                nextActivity.PutExtra("userid", UserList[ndx].Id);
                MainActivity.Instance.StartActivity(nextActivity);
            }
            return true;
        }

        public TextView TextNothingFound => v.FindViewById<TextView>(Resource.Id.TextNothingFound);
    }
}