using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Models.Purchase;
using Tokkepedia.Shared.Services.Interfaces;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using Xamarin.Essentials;

namespace Tokkepedia.Shared.Services
{
    public class TokMojiService : ITokMojiService
    {
        public static ITokMojiService Instance = new TokMojiService();
        private HttpClientHelper _httpClientHelper;
        public TokMojiService()
        {
            _httpClientHelper = new HttpClientHelper(Config.Configurations.BaseUrl);
        }
        /// <summary>Gets all tokmoji available for purchase. Used for the Tokmoji Shop</summary>
        public async Task<ResultData<Tokmoji>> GetTokmojisAsync(string paginationId = null)
        {
            var apiUrl = $"{Config.Configurations.ApiPrefix}/tokmojis{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            _httpClientHelper.ClearHeaders();
            if (!string.IsNullOrEmpty(paginationId))
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pagination_id", paginationId);

            var response = await _httpClientHelper.Instance.GetAsync(apiUrl).ConfigureAwait(false);
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultData<Tokmoji>>(content) ?? new ResultData<Tokmoji>() { Results = new List<Tokmoji>() };
            return result;
        }
        public async Task<ResultModel> PurchaseTokmojiAsync(string tokmojiid, string itemLabel)
        {
            var result = new ResultModel() { ResultEnum = Result.Failed, ResultMessage = "" };
            _httpClientHelper.ClearHeaders();

            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");

            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("tokmojiid", tokmojiid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("itemlabel", itemLabel);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/purchasecoins/{tokmojiid}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.PostAsync(apiUrl, tokmojiid).ConfigureAwait(false);

            var purchaseResult = JsonConvert.DeserializeObject<PurchaseResultModel>(response);
            if (purchaseResult != null)
            {
                //var purchaseResult = JsonConvert.DeserializeObject<PurchaseResultModel>(content);
                var resultserpurchase = JsonConvert.SerializeObject(purchaseResult.Content);
                var purchaseTokmoji = JsonConvert.DeserializeObject<TokkepediaResponse<PurchasedTokmoji>>(resultserpurchase);
                result.ResultEnum = purchaseResult.IsSuccess ? Result.Success : Result.Failed;
                result.ResultMessage = purchaseResult.Message;

                if (purchaseTokmoji != null)
                {
                    result.ResultObject = purchaseTokmoji.Resource;
                }
            }
            return result;
        }
    }
}