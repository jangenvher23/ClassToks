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

namespace Tokkepedia.Fragments
{
    public class categories_fragment : AndroidX.Fragment.App.Fragment, View.IOnTouchListener
    {

        Android.Views.View v;
        public string filterText { get; set; } = "";
        private List<Category> CategoryList;
        public RecyclerView RecyclerCategoriesContainer => v.FindViewById<RecyclerView>(Resource.Id.RecyclerContainer);
        private ProgressBar progressBar;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            CategoryList = new List<Category>();
            v = inflater.Inflate(Resource.Layout.container, container, false);


            progressBar = v.FindViewById<ProgressBar>(Resource.Id.progressbar);
            progressBar.Visibility = ViewStates.Visible;

            RecyclerCategoriesContainer.SetLayoutManager(new LinearLayoutManager(Application.Context));

            ((Activity)Context).RunOnUiThread(async () => await InitializeCategories());
            Settings.ActivityInt = 0;

            return v;
        }

        private async Task InitializeCategories()
        {
            var resultCategories = await CommonService.Instance.SearchCategoriesAsync(filterText);
            if (resultCategories.Results.Count() == 0)
            {
                TextNothingFound.Text = "No categories found.";
                TextNothingFound.Visibility = ViewStates.Visible;
            }
            else
            {
                TextNothingFound.Visibility = ViewStates.Gone;
            }

            CategoryList.AddRange(resultCategories.Results);
            SetCategoriesAdapter();
            progressBar.Visibility = ViewStates.Gone;
        }

        private void SetCategoriesAdapter()
        {
            var adapterCategories = CategoryList.GetRecyclerAdapter(BindCategoriesViewHolder, Resource.Layout.categoryrow);
            RecyclerCategoriesContainer.SetAdapter(adapterCategories);
        }

        private void BindCategoriesViewHolder(CachingViewHolder holder, Category model, int position)
        {
            var Category = holder.FindCachedViewById<TextView>(Resource.Id.TextCategory);
            var TokCounter = holder.FindCachedViewById<TextView>(Resource.Id.TokCounter);

            Category.Text = model.Name;
            TokCounter.Text = model.Toks.ToString() + " toks";

            Category.ContentDescription = position.ToString();
            Category.SetOnTouchListener(this);

        }
        public bool OnTouch(View v, MotionEvent e)
        {
            int ndx = int.Parse((string)v.ContentDescription);
            if (e.Action == MotionEventActions.Up)
            {
                bool gotonextpage = true;

                string titlepage = "";
                string filter = "";
                string headerpage = (v as TextView).Text;

                if (Settings.FilterTag == 3)
                {
                    gotonextpage = false;
                }

                if (gotonextpage)
                {
                    Settings.FilterTag = 3;
                    titlepage = "Category";
                    filter = CategoryList[ndx].Id;

                    Intent nextActivity = new Intent(MainActivity.Instance, typeof(ToksActivity));
                    nextActivity.PutExtra("titlepage", titlepage);
                    nextActivity.PutExtra("filter", filter);
                    nextActivity.PutExtra("headerpage", headerpage);
                    nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                    nextActivity.SetFlags(ActivityFlags.NewTask);
                    MainActivity.Instance.StartActivity(nextActivity);
                }
            }
            return true;
        }
        public TextView TextNothingFound => v.FindViewById<TextView>(Resource.Id.TextNothingFound);
    }
}