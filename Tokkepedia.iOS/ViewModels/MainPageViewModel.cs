using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia.Tools;
using UIKit;

namespace Tokkepedia.iOS.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {

        #region Properties
        public ObservableCollection<TokModel> tokModelLists { get; private set; }
        public FilterToks filterToks { get; set; }

        #endregion
        #region Commands
        public UICollectionView CollectionView { get; set; }
        #endregion

        public async Task InitAsync(FilterType type = FilterType.None)
        {
            tokModelLists = new ObservableCollection<TokModel>();
            tokModelLists.Clear();
            var resultToksData = await GetToksData("", type);

            foreach (var item in resultToksData)
            {
                tokModelLists.Add(item);
            }

            CollectionView.Source = new CustomCollectionSource(resultToksData);

            if (filterToks == FilterToks.Toks)
            {

            }
            else if (filterToks == FilterToks.Cards)
            {

            }
        }

        public async Task<List<Shared.Models.TokModel>> GetToksData(string filter = "", FilterType type = FilterType.None)
        {
            Settings.FilterTag = Convert.ToInt32(type);
            var tokResult = new List<Shared.Models.TokModel>();
            UserModel myAccount = JsonConvert.DeserializeObject<UserModel>(Settings.UserAccount);
            string strtoken = myAccount.StreamToken;
            switch (type)
            {
                case FilterType.None:
                    if (myAccount != null)
                        tokResult = await TokService.Instance.GetAllToks(new TokQueryValues() { streamtoken = strtoken });
                    else
                        tokResult = await TokService.Instance.GetAllFeaturedToks();
                    break;
                case FilterType.TokType:
                    tokResult = await TokService.Instance.GetAllToks(new TokQueryValues() { toktype = filter, streamtoken = strtoken });
                    break;
                case FilterType.Text:
                    tokResult = await TokService.Instance.GetAllToks(new TokQueryValues() { text = filter, streamtoken = strtoken });
                    break;
                case FilterType.Category:
                    tokResult = await TokService.Instance.GetAllToks(new TokQueryValues() { category = filter?.Replace("category-", ""), streamtoken = strtoken });
                    break;
                case FilterType.Country:
                    tokResult = await TokService.Instance.GetAllToks(new TokQueryValues() { country = filter, streamtoken = strtoken });
                    break;
                case FilterType.User:
                    tokResult = await TokService.Instance.GetAllToks(new TokQueryValues() { userid = filter, streamtoken = strtoken });
                    break;
                case FilterType.Group:
                    tokResult = await TokService.Instance.GetAllToks(new TokQueryValues() { tokgroup = filter, streamtoken = strtoken });
                    break;
                case FilterType.Featured:
                    tokResult = await TokService.Instance.GetAllFeaturedToks();
                    break;
                case FilterType.All:
                    tokResult = await TokService.Instance.GetAllToks(new TokQueryValues() { streamtoken = strtoken });
                    break;
                default:
                    tokResult = await TokService.Instance.GetAllToks(new TokQueryValues() { streamtoken = strtoken });
                    break;
            }
            var toksWithSticker = new List<TokModel>();
            tokResult = tokResult.OrderByDescending(x => x.DateCreated.Value).ToList() ?? new List<TokModel>();
            var cnt = 0;
            foreach (var tok in tokResult)
            {
                var sticker = StickersTool.Stickers.FirstOrDefault(x => x.Id == (string.IsNullOrEmpty(tok.Sticker) ? tok.Sticker : tok.Sticker.Split("-")[0]));
                tok.StickerImage = sticker?.Image ?? string.Empty;
                tok.IndexCounter = cnt;
                toksWithSticker.Add(tok);
                cnt += 1;
            }
            toksWithSticker = toksWithSticker.ToList();
            return tokResult;
        }

    }
}