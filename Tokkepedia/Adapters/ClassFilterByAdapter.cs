using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Newtonsoft.Json;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.ViewHolders;

namespace Tokkepedia.Adapters
{
    class ClassFilterByAdapter : RecyclerView.Adapter
    {
        View itemView;
        // Event handler for item clicks:
        public event EventHandler<int> ItemClick;
        Context context; List<CommonModel> listCommonModel;
        FilterBy filterByEnum = FilterBy.None;
        int typePosition;
        bool isSetSelected = true;

        public ClassFilterByAdapter(Context context, List<CommonModel> _listCommonModel, int filterByEnum, int typePosition = -1)
        {
            this.context = context;
            this.listCommonModel = _listCommonModel;
            this.filterByEnum = (FilterBy)filterByEnum;
            this.typePosition = typePosition;
        }

        public override int ItemCount => listCommonModel.Count;

        public override long GetItemId(int position)
        {
            return position;
        }


        ClassFilterByViewHolder vh; int selectedPosition = -1;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            vh = holder as ClassFilterByViewHolder;

            vh.txtClassFilter.Text = listCommonModel[position].Title;

            if (filterByEnum == FilterBy.Type)
            {
                if (typePosition >= 0)
                {
                    if (isSetSelected)
                    {
                        selectedPosition = typePosition;
                        isSetSelected = false;
                    }
                }
            }

            if (selectedPosition == position)
            {
                listCommonModel[position].isSelected = !listCommonModel[position].isSelected;

                //hightlight current selected row
                if (listCommonModel[position].isSelected)
                {
                    vh.ItemView.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.lightBlue)));
                }
                else
                {
                    vh.ItemView.SetBackgroundColor(Color.Transparent);
                }

                //var filterByList = listCommonModel.Where(x => x.isSelected == true).ToList();
                List<string> filterItems = new List<string>();
                foreach (var item in listCommonModel)
                {
                    if (item.isSelected)
                    {
                        string itemSelected = "";
                        switch (filterByEnum)
                        {
                            case FilterBy.Class:
                                itemSelected = item.Id;
                                break;
                            case FilterBy.Category:
                                itemSelected = item.Id;
                                break;
                            case FilterBy.Type:
                                itemSelected = item.Title;
                                break;
                            default:
                                break;
                        }
                        filterItems.Add(itemSelected);
                    }
                }

                ClassFilterbyActivity.Instance.btnApplyFilter.ContentDescription = JsonConvert.SerializeObject(filterItems);
            }

            vh.ItemView.Click += (sender, e) =>
            {
                selectedPosition = position;
                NotifyDataSetChanged();
            };
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.settings_row, parent, false);

            vh = new ClassFilterByViewHolder(itemView, OnClick);

            return vh;
        }
        public override int GetItemViewType(int position)
        {
            return position;

            //This was added due to the reason that when clicked, there will be 2 rows that will highlight
            //For Reference, this one have similar issue with the link below
            //https://stackoverflow.com/questions/32065267/recyclerview-changing-items-during-scroll
        }
        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }
    }
}