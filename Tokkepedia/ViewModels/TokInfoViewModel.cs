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
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using GalaSoft.MvvmLight;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;

namespace Tokkepedia.ViewModels
{
    public class TokInfoViewModel : ViewModelBase
    {
        public ObservableCollection<ReactionModel> CommentsCollection { get; private set; }
        public ObservableCollection<Tokmoji> TokMojiCollection { get; private set; }
        public ProgressBar CircleProgress { get; set; }
        public int commentsloaded = 0;
        public TokInfoViewModel()
        {
            CommentsCollection = new ObservableCollection<ReactionModel>();
            CommentsCollection.Clear();

            TokMojiCollection = new ObservableCollection<Tokmoji>();
            TokMojiCollection.Clear();
        }
        public async Task LoadComments(string tokid,string continuationtoken = "")
        {
            ReactionQueryValues reactionQueryValues = new ReactionQueryValues();
            reactionQueryValues.item_id = tokid;
            reactionQueryValues.kind = "comments";
            reactionQueryValues.detail_number = -1;
            reactionQueryValues.pagination_id = continuationtoken;

            if (!string.IsNullOrEmpty(continuationtoken))
            {
                //Load More
                CircleProgress.Visibility = ViewStates.Visible;
            }

            var resultComments = await ReactionService.Instance.GetReactionsAsync(reactionQueryValues);
            resultComments.Results = resultComments.Results.OrderByDescending(x => x.CreatedTime).ToList();
            foreach (var comment in resultComments.Results)
            {
                CommentsCollection.Add(comment);
            }

            if (!string.IsNullOrEmpty(continuationtoken))
            {
                commentsloaded += resultComments.Results.Count();
                CircleProgress.Visibility = ViewStates.Gone;
            }
        }
        public async Task LoadTokMoji()
        {
            //var resultTokMoji = PurchasesHelper.GetProducts().Where(x => x.ProductType == ProductType.Tokmoji).ToList();
            var resultTokMoji = await TokMojiService.Instance.GetTokmojisAsync();
            var resultList = resultTokMoji.Results.ToList();
            foreach (var stickers in resultList)
            {
                TokMojiCollection.Add(stickers);
            }
        }
    }
}