using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Helpers.Interfaces;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services.Interfaces;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using Xamarin.Essentials;

namespace Tokkepedia.Shared.Services
{
    public class TokService : ITokService
    {
        public static ITokService Instance = new TokService();
        private HttpClientHelper _httpClientHelper;
        public TokService()
        {
            _httpClientHelper = new HttpClientHelper(Config.Configurations.BaseUrl);
        }

        public async Task<List<TokModel>> GetAllFeaturedToks()
        {
            var apiUrl = $"{Config.Configurations.ApiPrefix}/allfeaturedtoks{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.GetAsync(apiUrl);
            try
            {
                var data = JsonConvert.DeserializeObject<ResultData<Tok>>(response);
                Settings.ContinuationToken = data.ContinuationToken;
                var list = data.Results.ToList();
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].UserId == "tokket")
                    {
                        list[i].UserPhoto = "/images/tokket.png";
                    }
                }
                return JsonConvert.DeserializeObject<List<TokModel>>(JsonConvert.SerializeObject(list));
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<TokModel>> GetAllToks(TokQueryValues values)
        {
            if (values == null)
                values = new TokQueryValues();
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("order", values?.order);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("country", values?.country);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("category", values?.category);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("tokgroup", values?.tokgroup);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("toktype", values?.toktype);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", values?.userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("text", values?.text);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("loadmore", values?.loadmore);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", values?.token);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("streamtoken", values?.streamtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("sortby", values?.sortby);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("image", (values?.image).ToString());
            var apiUrl = $"{Config.Configurations.ApiPrefix}/toks{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.GetAsync(apiUrl);
            try
            {
                var data = JsonConvert.DeserializeObject<ResultData<Tok>>(response);
                Settings.ContinuationToken = data.ContinuationToken;
                var list = data.Results.ToList();
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].UserId == "tokket")
                    {
                        list[i].UserPhoto = "/images/tokket.png";
                    }
                }
                var serialize = JsonConvert.SerializeObject(list);
                return JsonConvert.DeserializeObject<List<TokModel>>(serialize);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<TokModel> AlternateToks(List<TokModel> resultData)
        {
            List<TokModel> image = new List<TokModel>(), nonImage = new List<TokModel>(), alternated = new List<TokModel>();
            image = resultData.Where(x => !(x.Image == null || x.Image.Equals(""))).ToList();
            nonImage = resultData.Where(x => (x.Image == null || x.Image.Equals(""))).ToList();
            //Image first
            while (image.Count > 0)
            {
                alternated.Add(image[image.Count - 1]);
                image.Remove(image[image.Count - 1]);
                //Non image next
                if (nonImage.Count > 0)
                {
                    alternated.Add(nonImage[nonImage.Count - 1]);
                    nonImage.Remove(nonImage[nonImage.Count - 1]);
                }
            }
            //Rest are non image
            if (nonImage.Count > 0)
                alternated.AddRange(nonImage);
            resultData = alternated;
            return resultData;
        }
        public async Task<List<TokModel>> GetToksAsync(TokQueryValues values = null)
        {
            if (values == null)
                values = new TokQueryValues();

            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("userid"); // Remove default
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("token"); // Remove default
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("streamtoken"); // Remove default

            _httpClientHelper.Instance.DefaultRequestHeaders.Add("order", values?.order);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("country", values?.country);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("category", values?.category);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("tokgroup", values?.tokgroup);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("toktype", values?.toktype);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", values?.userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("text", values?.text);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("loadmore", values?.loadmore);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", values?.token);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("streamtoken", values?.streamtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("sortby", values?.sortby);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("image", (values?.image).ToString());
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("tagid", (values?.tagid));
            var apiUrl = $"{Config.Configurations.ApiPrefix}/toks{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.GetAsync(apiUrl);
            try
            {
                var data = JsonConvert.DeserializeObject<ResultData<Tok>>(response);
                Settings.ContinuationToken = data.ContinuationToken;
                var list = data.Results.ToList();
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].UserId == "tokket")
                    {
                        list[i].UserPhoto = "/images/tokket.png";
                    }
                }
                var serialize = JsonConvert.SerializeObject(list);
                return JsonConvert.DeserializeObject<List<TokModel>>(serialize);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<TokModel>> GetAllFeaturedToksAsync()
        {
            _httpClientHelper.ClearHeaders();
            var apiUrl = $"{Config.Configurations.ApiPrefix}/allfeaturedtoks{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.GetAsync(apiUrl);
            try
            {
                var data = JsonConvert.DeserializeObject<ResultData<TokModel>>(response);
                Settings.ContinuationToken = data.ContinuationToken;

                var list = data.Results.ToList();
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].UserId == "tokket")
                    {
                        list[i].UserPhoto = "/images/tokket.png";
                    }
                }
                var serialize = JsonConvert.SerializeObject(list);
                return JsonConvert.DeserializeObject<List<TokModel>>(serialize);
            }
            catch
            {
                return null;
            }
        }
        public async Task<ResultModel> CreateTokAsync(TokModel tok, CancellationToken cancellationToken = default(CancellationToken))
        {
            var idtoken = await SecureStorage.GetAsync("idtoken");
            var apiUrl = $"{Config.Configurations.ApiPrefix}/tok{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", tok.UserId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("tokgroupid", tok.TokGroup.ToIdFormat());
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("toktypeid", tok.TokTypeId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("categoryid", tok.CategoryId);

            var model = JsonConvert.SerializeObject(tok);
            ResultModel result = new ResultModel() { ResultEnum = Result.Failed };

            try
            {
                var response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"), cancellationToken);
                var content = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.Accepted:
                    case System.Net.HttpStatusCode.OK:
                    case System.Net.HttpStatusCode.Created:
                        result.ResultEnum = Result.Success;
                        break;
                    case System.Net.HttpStatusCode.BadRequest:
                    case System.Net.HttpStatusCode.Forbidden:
                    case System.Net.HttpStatusCode.NotFound:
                    case System.Net.HttpStatusCode.Unauthorized:
                        result.ResultEnum = Result.Failed;
                        break;
                }

                if (result.ResultEnum == Result.Success)
                {
                    result.ResultObject = (!string.IsNullOrEmpty(content) ? JsonConvert.DeserializeObject<TokModel>(content) : null);
                }
                else
                {
                    result.ResultObject = null;
                }
            }
            catch (Exception ex)
            {
                result.ResultEnum = Result.Failed;
                result.ResultMessage = "cancelled";
            }

            return result;
        }
        public async Task<ResultModel> DeleteTokAsync(string id)
        {
            var apiUrl = $"{Config.Configurations.ApiPrefix}/tok/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.DeleteAsync(apiUrl);
            ResultModel result = new ResultModel() { ResultEnum = Result.Failed };
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.Accepted:
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.Created:
                    result.ResultEnum = Result.Success;
                    break;
                case System.Net.HttpStatusCode.BadRequest:
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.NotFound:
                case System.Net.HttpStatusCode.Unauthorized:
                    result.ResultEnum = Result.Failed;
                    break;
            }

            return result;
        }
        public async Task<ResultModel> UpdateTokAsync(TokModel tok, CancellationToken cancellationToken = default(CancellationToken))
        {
            ResultModel result = new ResultModel() { ResultEnum = Result.Failed, ResultMessage = "Updating tok failed." };
            var apiUrl = $"{Config.Configurations.ApiPrefix}/tok/{tok.Id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var model = JsonConvert.SerializeObject(tok);

            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", tok.UserId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
           try
            {
                var response = await _httpClientHelper.Instance.PutAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"), cancellationToken);
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    result.ResultEnum = Result.Success;
                    result.ResultMessage = "Update Successful!";
                    result.ResultObject = (!string.IsNullOrEmpty(content) ? JsonConvert.DeserializeObject<Tok>(content) : null);
                }
            }
            catch (Exception ex)
            {
                result.ResultEnum = Result.Failed;
                result.ResultMessage = "cancelled";
            }
            return result;
        }
        public async Task<TokModel> GetTokIdAsync(string id)
        {
            var apiUrl = $"{Config.Configurations.ApiPrefix}/tok/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.GetAsync(apiUrl);
            return JsonConvert.DeserializeObject<TokModel>(response);
        }
        public async Task<List<TokModel>> GetToksByIdsAsync(List<string> ids)
        {
            if (ids == null)
                return null;
            if (ids.Count > 100)
                return null;
            var apiUrl = $"{Config.Configurations.ApiPrefix}/toks/ids{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.PostAsync(apiUrl, ids);
            try
            {
                var data = JsonConvert.DeserializeObject<ResultData<Tok>>(response);
                Settings.ContinuationToken = data.ContinuationToken;
                if (data.Results != null)
                {
                    var list = data.Results.ToList();
                    for (int i = 0; i < list.Count; ++i)
                    {
                        if (list[i].UserId == "tokket")
                        {
                            list[i].UserPhoto = "/images/tokket.png";
                        }
                    }
                    var serialize = JsonConvert.SerializeObject(list);
                    return JsonConvert.DeserializeObject<List<TokModel>>(serialize);
                }
                else
                {
                    return null;
                }
                
            }
            catch
            {
                return null;
            }

        }

        #region MegaTok
        public async Task<ResultData<TokSection>> GetTokSectionsAsync(string tokId, int count = 0, string continuationToken = null)
        {
            TokSectionQueryValues values = new TokSectionQueryValues()
            {
                tokId = tokId,
                continuationToken = continuationToken,
                count = count
            };
            Settings.ContinuationToken = values.continuationToken;
            var apiUrl = $"{Config.Configurations.ApiPrefix}/toksections/{tokId}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            
            var model = JsonConvert.SerializeObject(values);
            var response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();
            ResultData<TokSection> result = new ResultData<TokSection>();
            result = JsonConvert.DeserializeObject<ResultData<TokSection>>(content);
            if (result.Results == null)
            {
                return new ResultData<TokSection>() { Results = new List<TokSection>() };
            }
            else
            {
                return result; //.ToList()
            }
            
        }
        public async Task<bool> CreateTokSectionAsync(TokSection tokSection, string tokId, int partitionNumber)
        {
            string partitionKey = $"{tokId}-toksections{partitionNumber}";
            tokSection.PartitionKey = partitionKey;

            _httpClientHelper.ClearHeaders();
           var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", tokSection.UserId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", partitionKey);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/toksection/create/{tokId}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            
            var model = JsonConvert.SerializeObject(tokSection);
            var response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"));
            var content = response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> UpdateTokSectionAsync(TokSection newTokSection)
        {
            if (string.IsNullOrEmpty(newTokSection.PartitionKey) || string.IsNullOrEmpty(newTokSection.TokId))
                return false;
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("pk"); // Avoids duplicate pk, get tok section will cause duplicate pk     
                                                                           //Required
            _httpClientHelper.ClearHeaders();
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", newTokSection.UserId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", newTokSection.PartitionKey);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("tokid", newTokSection.TokId);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/toksection/{newTokSection.Id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var model = JsonConvert.SerializeObject(newTokSection);
            var response = await _httpClientHelper.Instance.PutAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"));
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> DeleteTokSectionAsync(TokSection tokSection)
        {
            if (string.IsNullOrEmpty(tokSection.Id) || string.IsNullOrEmpty(tokSection.TokId) || string.IsNullOrEmpty(tokSection.PartitionKey))
                return false;
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("pk"); // Avoids duplicate pk, get tok section will cause duplicate pk
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", tokSection.PartitionKey);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("tokid", tokSection.TokId);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/toksection/{tokSection.Id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.DeleteAsync(apiUrl);
            return response.IsSuccessStatusCode;
        }
        public async Task<ResultData<TokkepediaReaction>> UserReactionsGet(string item_id)
        {
            _httpClientHelper.ClearHeaders();
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken); 
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", Settings.GetUserModel().UserId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("item_id", item_id);
            
            var apiUrl = $"{Config.Configurations.ApiPrefix}/userreactions/{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.GetAsync(apiUrl);
            return JsonConvert.DeserializeObject<ResultData<TokkepediaReaction>>(response);
        }
        #endregion

    }
}