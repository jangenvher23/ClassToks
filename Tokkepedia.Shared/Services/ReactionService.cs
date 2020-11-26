using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services.Interfaces;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using Xamarin.Essentials;

namespace Tokkepedia.Shared.Services
{
    public class ReactionService : IReactionService
    {
        public static IReactionService Instance = new ReactionService();
        private HttpClientHelper _httpClientHelper;
        public ReactionService()
        {
            _httpClientHelper = new HttpClientHelper(Config.Configurations.BaseUrl);
        }
        public async Task<ResultModel> AddReaction(ReactionModel item)
        {
            _httpClientHelper.ClearHeaders();
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", Settings.GetUserModel().UserId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("itemid", item.ItemId);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/reaction{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var model = JsonConvert.SerializeObject(item);

            ResultModel result = new ResultModel() { ResultEnum = Result.Failed };
            try
            {
                var response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                var resultModel = JsonConvert.DeserializeObject<ResultModel>(content);
                
                result.ResultObject = (!string.IsNullOrEmpty(content) ? JsonConvert.DeserializeObject<ResultModel>(content) : null);
                result.ResultEnum = resultModel.ResultEnum;
            }
            catch (Exception ex)
            {
                result.ResultObject = null;
                result.ResultEnum = Result.None;
            }
            
            return result;
        }

        public async Task<bool> UpdateReaction(TokkepediaReaction item)
        {
            //Settings.GetUserModel().UserId
            _httpClientHelper.ClearHeaders();
            //var idtoken = await SecureStorage.GetAsync("idtoken");
            //_httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            //_httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", Settings.GetUserModel().UserId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", item.PartitionKey);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/reaction/{item.Id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var model = JsonConvert.SerializeObject(item);
            var response = await _httpClientHelper.Instance.PutAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteReaction(string id)
        {
            _httpClientHelper.ClearHeaders();
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", Settings.GetUserModel().UserId);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/reaction/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.DeleteAsync(apiUrl);
            var test = response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        public async Task<ResultData<ReactionModel>> GetReactionsAsync(ReactionQueryValues values = null)
        {
            if (values == null)
                values = new ReactionQueryValues();
            _httpClientHelper.ClearHeaders();
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("limit", values?.limit.ToString());
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("kind", values?.kind);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("item_id", values?.item_id);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("activity_id", values?.activity_id);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("user_id", values?.user_id);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("reaction_id", values?.reaction_id);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pagination_id", values?.pagination_id);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("reaction_total", values?.reaction_total.ToString() ?? "0");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("detail_number", values?.detail_number.ToString() ?? "0");
            var apiUrl = $"{Config.Configurations.ApiPrefix}/reactions{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.GetAsync(apiUrl);

            try
            {
                var data = JsonConvert.DeserializeObject<ResultData<ReactionModel>>(response);
                Settings.ContinuationToken = data.ContinuationToken;
                var list = data.Results.ToList();
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].UserId == "tokket")
                    {
                        list[i].UserPhoto = "/images/tokket.png";
                    }
                }

                return data;// JsonConvert.DeserializeObject<List<ReactionModel>>(JsonConvert.SerializeObject(list));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ReactionValueModel> GetReactionsValueAsync(string id)
        {

            var apiUrl = $"{Config.Configurations.ApiPrefix}/reactioncounters/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}&gemcounter=true&commentcounter=true&viewcounter=true&serviceid={Config.Configurations.ServiceId}&deviceplatform={Config.Configurations.DevicePlatform}";
            var response = await _httpClientHelper.GetAsync(apiUrl);

            try
            {
                var data = JsonConvert.DeserializeObject<List<object>>(response);

                var gem = JsonConvert.DeserializeObject<GemsModel>(JsonConvert.SerializeObject(data[0]));
                var comments = JsonConvert.DeserializeObject<CommentsModel>(JsonConvert.SerializeObject(data[1]));
                var views = JsonConvert.DeserializeObject<ViewsModel>(JsonConvert.SerializeObject(data[2]));

                ReactionValueModel reactionValueModel = new ReactionValueModel();
                reactionValueModel.GemsModel = gem;
                reactionValueModel.CommentsModel = comments;
                reactionValueModel.ViewsModel = views;

                return reactionValueModel;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<TokketUserReaction>> GetReactionsUsersAsync(ReactionQueryValues reactionQueryValues)
        {
            _httpClientHelper.ClearHeaders();
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("item_id", reactionQueryValues.item_id);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("detail_number", reactionQueryValues.detail_number.ToString());
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("kind", reactionQueryValues.kind);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pagination_id", reactionQueryValues.pagination_id);

            var apiUrl = $"{Config.Configurations.ApiPrefix}/reactionsusers{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.GetAsync(apiUrl);

            try
            {
                //var data = JsonConvert.DeserializeObject<List<TokketUserReaction>>(response);
                //return data;

                var data = JsonConvert.DeserializeObject<ResultData<TokketUserReaction>>(response);
                Settings.ContinuationToken = data.ContinuationToken;
                var list = data.Results.ToList();
                return JsonConvert.DeserializeObject<List<TokketUserReaction>>(JsonConvert.SerializeObject(list));
            }
            catch
            {
                return null;
            }
        }

        public async Task<ResultData<ReactionModel>> GetCommentReplyAsync(ReactionQueryValues values = null)
        {
            if (values == null)
                values = new ReactionQueryValues();

            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("item_id", values?.item_id);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("detail_number", values?.detail_number.ToString() ?? "-1");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("kind", values?.kind);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("reaction_id", values?.reaction_id);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("limit", values?.limit.ToString());
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("activity_id", values?.activity_id);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("user_id", values?.user_id);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pagination_id", values?.pagination_id);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("reaction_total", values?.reaction_total.ToString() ?? "");


            var apiUrl = $"{Config.Configurations.ApiPrefix}/reactionreplies{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.GetAsync(apiUrl);

            try
            {
                var data = JsonConvert.DeserializeObject<ResultData<ReactionModel>>(response);
                return data;
                //Settings.ContinuationToken = data.ContinuationToken;
                //var list = data.Results.ToList();
                //for (int i = 0; i < list.Count; ++i)
                //{
                //    if (list[i].UserId == "tokket")
                //    {
                //        list[i].UserPhoto = "/images/tokket.png";
                //    }
                //}
                //return JsonConvert.DeserializeObject<List<TokkepediaReaction>>(JsonConvert.SerializeObject(list));
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<TokkepediaReaction> GetCommentAsync(string id)
        {
            _httpClientHelper.ClearHeaders();
            var apiUrl = $"{Config.Configurations.ApiPrefix}/reactions/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);
            var content = response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<TokkepediaReaction>(content.Result);

            return data;
        }

    }
}