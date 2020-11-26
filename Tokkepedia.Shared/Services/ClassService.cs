using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services.Interfaces;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using Xamarin.Essentials;

namespace Tokkepedia.Shared.Services
{
    public class ClassService : IClassService
    {
        public static IClassService Instance = new ClassService();
        private HttpClientHelper _httpClientHelper;
        public ClassService()
        {
            _httpClientHelper = new HttpClientHelper(Config.Configurations.BaseUrl);
        }

        #region  Class Group
        public async Task<ClassGroupModel> AddClassGroupAsync(ClassGroupModel item)
        {
            item.Label = "classgroup";
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgroup{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            
            var model = JsonConvert.SerializeObject(item);
            _httpClientHelper.ClearHeaders();
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);

            var response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"));

            var data = await response.Content.ReadAsStringAsync();
            var dataresource = JsonConvert.DeserializeObject<TokkepediaResponse<Dictionary<string, object>>>(data);
            var resource = JsonConvert.SerializeObject(dataresource.Resource, Formatting.Indented);
            var result = JsonConvert.DeserializeObject<ClassGroupModel>(resource);
            return result;
        }

        public async Task<bool> UpdateClassGroupAsync(ClassGroupModel item)
        {
            _httpClientHelper.ClearHeaders();
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", item.PartitionKey);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgroup/{item.Id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            var model = JsonConvert.SerializeObject(item);
            var response = await _httpClientHelper.Instance.PutAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteClassGroupAsync(string id, string pk)
        {
            _httpClientHelper.ClearHeaders();
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", pk);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgroup/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.DeleteAsync(apiUrl);
            var test = response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        public async Task<ClassGroupModel> GetClassGroupAsync(string id)
        {
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgroup/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);
            var data = await response.Content.ReadAsStringAsync();
            var dataresource = JsonConvert.DeserializeObject<TokkepediaResponse<Dictionary<string, object>>>(data);
            var resource = JsonConvert.SerializeObject(dataresource.Resource, Formatting.Indented);
            var result = JsonConvert.DeserializeObject<ClassGroupModel>(resource);
            return result;
        }

        public async Task<ResultData<ClassGroupModel>> GetClassGroupAsync(ClassGroupQueryValues queryValues)
        {
            //var idToken = await SecureStorage.GetAsync("idtoken");

            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("userid");
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("pk");
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("text");
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("joined");
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("paginationid");
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("showimage");
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("isdescending");

            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", queryValues.userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", queryValues.partitionkeybase);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("text", queryValues.text);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("joined", queryValues.joined.ToString());
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("paginationid", queryValues.paginationid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("showimage", queryValues.showImage?.ToString() ?? string.Empty);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("isdescending", queryValues.isDescending?.ToString() ?? string.Empty);


            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgroups{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);
            var data = await response.Content.ReadAsStringAsync();
            //var dataresource = JsonConvert.DeserializeObject<TokkepediaResponse<Dictionary<string, object>>>(data);
            //var resource = JsonConvert.SerializeObject(dataresource.Resource, Formatting.Indented);
            var result = JsonConvert.DeserializeObject<ResultData<ClassGroupModel>>(data);
            return result;
        }

        public async Task<List<CommonModel>> GetMoreFilterOptions(ClassTokQueryValues queryValues)
        {
            _httpClientHelper.ClearHeaders();
            if (queryValues.FilterBy == FilterBy.Type)
            {
                return new List<CommonModel>() {
                    new CommonModel() { LabelIdentifier = "classtokgroup", Title = "Basic", Description = "Basic" },
                    new CommonModel() { LabelIdentifier = "classtokgroup", Title = "Detailed", Description = "Detailed" },
                    new CommonModel() { LabelIdentifier = "classtokgroup", Title = "Mega", Description = "Mega" }
                };
            }
            else
            {
                _httpClientHelper.Instance.DefaultRequestHeaders.Add("filterby", queryValues?.FilterBy.ToString());
                _httpClientHelper.Instance.DefaultRequestHeaders.Add("recent", queryValues?.RecentOnly.ToString().ToLower());
                var apiUrl = $"{Config.Configurations.ApiPrefix}/filterby/{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
                HttpResponseMessage response = await _httpClientHelper.Instance.GetAsync(apiUrl);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ResultModel>(content);
                return result.ResultObject != null ? JsonConvert.DeserializeObject<List<CommonModel>>(result.ResultObject.ToString()) : new List<CommonModel>();
            }
        }
        #endregion


        #region Class Group Request

        public async Task<ClassGroupRequestModel> GetClassGroupRequestAsync(string id, string pk)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", pk);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgrouprequest/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);

            var data = await response.Content.ReadAsStringAsync();

            var dataresource = JsonConvert.DeserializeObject<TokkepediaResponse<Dictionary<string, object>>>(data);
            var resource = JsonConvert.SerializeObject(dataresource.Resource, Formatting.Indented);
            var result = JsonConvert.DeserializeObject<ClassGroupRequestModel>(resource);
            return result;
        }

        public async Task<ClassGroupRequestModel> RequestClassGroupAsync(ClassGroupRequestModel item)
        {
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgrouprequest{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);

            var modelConvert = JsonConvert.SerializeObject(item);
            var response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(modelConvert, Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<TokkepediaResponse<Dictionary<string, object>>>(content);
            var resource = JsonConvert.SerializeObject(data.Resource, Formatting.Indented);
            var result = JsonConvert.DeserializeObject<ClassGroupRequestModel>(resource);
            return result;
        }

        public async Task<ResultData<ClassGroupRequestModel>> GetClassGroupJoinRequests(string continuationtoken, string groupid, RequestStatus requestStatus = RequestStatus.Pending)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("continuationtoken", continuationtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", "classgrouprequests");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("groupid", groupid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("status", ((int)requestStatus).ToString());
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgrouprequests/{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);
            var content = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<TokkepediaResponse<Dictionary<string, object>>>(content);
            var resource = JsonConvert.SerializeObject(data.Resource, Formatting.Indented);
            var result = JsonConvert.DeserializeObject<ResultData<ClassGroupRequestModel>>(resource);
            return result;
        }

        public async Task<ResultData<ClassGroupModel>> GetClassGroupRequestAsync(ClassGroupRequestQueryValues queryValues)
        {
            var idToken = await SecureStorage.GetAsync("idtoken");

            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", queryValues.partitionkeybase);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("receiverid", queryValues.receiverid);
            
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgrouprequests/{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
        
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);
            var content = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<TokkepediaResponse<ResultData<Dictionary<string, object>>>>(content);
            var resource = JsonConvert.SerializeObject(data.Resource, Formatting.Indented);
            var result = JsonConvert.DeserializeObject<ResultData<ClassGroupModel>>(resource);
            return result;

        }

        public async Task<bool> AcceptRequest(string id, string pk, ClassGroupModel model)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", pk);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgrouprequestaccept/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var modelConvert = JsonConvert.SerializeObject(model);
            var response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(modelConvert, Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeclineRequest(string id, string pk, ClassGroupModel model)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", pk);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgrouprequestdecline/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            var modelConvert = JsonConvert.SerializeObject(model);
            var response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(modelConvert, Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LeaveClassGroupAsync(string id, string pk)
        {
            _httpClientHelper.ClearHeaders();
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", pk);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgroupleave/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.PostAsync(apiUrl, id);
            if (response.Contains("200"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public async Task<ResultData<TokketUser>> GetGroupMembers(string groupid, string paginationid = "")
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pagination_id", paginationid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", groupid + "-classgroupmembers");
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classgroupmembers/{groupid}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
 
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);

            var data = await response.Content.ReadAsStringAsync();
            var dataresource = JsonConvert.DeserializeObject<TokkepediaResponse<ResultData<Dictionary<string, object>>>>(data);
            var resource = JsonConvert.SerializeObject(dataresource.Resource, Formatting.Indented);
            var result = JsonConvert.DeserializeObject<ResultData<TokketUser>>(resource);
            return result;
        }



        #endregion


        #region Class Set
        public async Task<bool> AddClassSetAsync(ClassSetModel item, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool isSuccess = true;
            item.Label = "classset";
            _httpClientHelper.ClearHeaders();
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);

            var apiUrl = $"{Config.Configurations.ApiPrefix}/classset{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var model = JsonConvert.SerializeObject(item);
            try
            {
                var response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"), cancellationToken);
                var content = await response.Content.ReadAsStringAsync();
                isSuccess = true; //response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            return isSuccess;
        }

        public async Task<bool> UpdateClassSetAsync(ClassSetModel item, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool isSuccess = true;
            _httpClientHelper.ClearHeaders();
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", item.UserId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", item.PartitionKey);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classset/{item.Id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            var model = JsonConvert.SerializeObject(item);
           try
            {
                var response = await _httpClientHelper.Instance.PutAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                isSuccess = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                isSuccess = false;
            }
            return isSuccess;
        }

        public async Task<bool> DeleteClassSetAsync(string id, string pk)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", pk);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classset/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.DeleteAsync(apiUrl);
            var test = response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        public async Task<ClassSetModel> GetClassSetAsync(string id, string pk)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", pk);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classset/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);

            var data = await response.Content.ReadAsStringAsync();
            var dataresource = JsonConvert.DeserializeObject<TokkepediaResponse<ClassSetModel>>(data);
            return dataresource.Resource;
        }

        public async Task<ResultData<ClassSetModel>> GetClassSetAsync(ClassSetQueryValues queryValues, CancellationToken cancellationToken = default(CancellationToken))
        {
            _httpClientHelper.ClearHeaders();
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", queryValues.userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", queryValues.partitionkeybase);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("groupid", queryValues.groupid);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classsets/{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            ResultData<ClassSetModel> result = new ResultData<ClassSetModel>();
            try
            {
                var response = await _httpClientHelper.Instance.GetAsync(apiUrl, cancellationToken);
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<TokkepediaResponse<ResultData<Dictionary<string, object>>>>(content);
                var resource = JsonConvert.SerializeObject(data.Resource, Formatting.Indented);

                result = JsonConvert.DeserializeObject<ResultData<ClassSetModel>>(resource);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result.ContinuationToken = "cancelled";
            }

            return result;
        }
        #endregion


        #region Class Tok

        public async Task<ResultModel> AddClassToksAsync(ClassTokModel item, CancellationToken cancellationToken = default(CancellationToken))
        {
            //item.Label = "classtok";
            ResultModel result = new ResultModel() { ResultEnum = Result.Failed, ResultMessage = "Creating tok failed." };
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classtok{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            _httpClientHelper.ClearHeaders();
            var idtoken = await SecureStorage.GetAsync("idtoken") ?? "test";
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", item.UserId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("tokgroupid", item.TokGroup.ToIdFormat());
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("toktypeid", item.TokTypeId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("categoryid", item.CategoryId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("private", item.IsPrivate.ToString().ToLower());
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("group", item.IsGroup.ToString().ToLower());
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("public", item.IsPublic.ToString().ToLower());
            var model = JsonConvert.SerializeObject(item);
            try
            {
                var response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"), cancellationToken);
                var content = await response.Content.ReadAsStringAsync();
                var resultContent = JsonConvert.DeserializeObject<ResultModel>(content);
                result.ResultEnum = resultContent.ResultEnum;
                result.ResultMessage = resultContent.ResultMessage;
                result.ResultObject = resultContent.ResultObject;
            }
            catch (Exception ex)
            {
                result.ResultEnum = Result.Failed;
                result.ResultMessage = "cancelled";
            }

            return result;

            ////var model = JsonConvert.SerializeObject(item);
            ////var secondaryVal = new StringContent(model, Encoding.UTF8, "application/json");
            //var response = await _httpClientHelper.PostAsync(apiUrl, item);
            //result = JsonConvert.DeserializeObject<ResultModel>(response);
            //if (result == null)
            //{
            //    result = new ResultModel();
            //    result.ResultEnum = Result.Success;
            //    result.ResultMessage = "Created tok successfully!";
            //}
            ////var content = await response.Content.ReadAsStringAsync();

            ////if (response.IsSuccessStatusCode)
            ////{
            ////    if (!string.IsNullOrEmpty(content))
            ////    {
            ////        result.ResultEnum = Result.Success;
            ////        result.ResultMessage = "Create Successful!";
            ////        result.ResultObject = JsonConvert.DeserializeObject<ClassTokModel>(content);
            ////    }
            ////}
            //return result;
        }

        public async Task<ResultModel> UpdateClassToksAsync(ClassTokModel item, CancellationToken cancellationToken = default(CancellationToken))
        {
            ResultModel result = new ResultModel() { ResultEnum = Result.Failed, ResultMessage = "Class Tok Update Failed." };

            _httpClientHelper.ClearHeaders();
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", item.PartitionKey);

            var apiUrl = $"{Config.Configurations.ApiPrefix}/classtok/{item.Id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            var model = JsonConvert.SerializeObject(item);

            try
            {
                var response = await _httpClientHelper.Instance.PutAsync(apiUrl, new StringContent(model, Encoding.UTF8, "application/json"), cancellationToken);
                var content = await response.Content.ReadAsStringAsync();

                var resultContent = JsonConvert.DeserializeObject<ResultModel>(content);
                if (resultContent != null)
                {
                    result.ResultEnum = resultContent.ResultEnum;
                    result.ResultMessage = resultContent.ResultMessage;
                    result.ResultObject = resultContent.ResultObject;
                }
            }
            catch (Exception ex)
            {
                result.ResultEnum = Result.Failed;
                result.ResultMessage = "cancelled";
            }

            return result;
        }

        public async Task<bool> DeleteClassToksAsync(string id, string pk)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", pk);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classtok/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.DeleteAsync(apiUrl);
            var test = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        public async Task<ClassTokModel> GetClassToksAsync(string id, string pk)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", pk);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classtoks/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);
            var content = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<TokkepediaResponse<ClassTokModel>>(content);
            return data.Resource;
        }

        public async Task<ResultData<ClassTokModel>> GetClassToksAsync(ClassTokQueryValues queryValues, CancellationToken token = default(CancellationToken))
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("pk");
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("groupid");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", queryValues?.partitionkeybase ?? string.Empty);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("groupid", queryValues?.groupid ?? string.Empty);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("public", queryValues?.publicfeed.ToString().ToLower());
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("paginationid", queryValues?.paginationid ?? string.Empty);

            if (!string.IsNullOrEmpty(queryValues?.userid))
            {
                _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", queryValues?.userid.ToString() ?? string.Empty);
            }

            if (!string.IsNullOrEmpty(queryValues?.searchvalue))
            {
                queryValues.searchkey = "primary_text"; // Currently the Value is always primary_text. To be changed in the future. 
                _httpClientHelper.Instance.DefaultRequestHeaders.Add("searchkey", queryValues?.searchkey ?? string.Empty);
                _httpClientHelper.Instance.DefaultRequestHeaders.Add("searchvalue", queryValues?.searchvalue ?? string.Empty);
            }
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("filterby", queryValues?.FilterBy.ToString() ?? string.Empty);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("filteritems", JsonConvert.SerializeObject(queryValues?.FilterItems ?? new List<string>()));

            var apiUrl = $"{Config.Configurations.ApiPrefix}/classtoks{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            ResultData<ClassTokModel> result = new ResultData<ClassTokModel>();
            try
            {
                HttpResponseMessage response = await _httpClientHelper.Instance.GetAsync(apiUrl, token);
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<TokkepediaResponse<ResultData<Dictionary<string, object>>>>(content);
                var resource = JsonConvert.SerializeObject(data.Resource, Formatting.Indented);

                result = JsonConvert.DeserializeObject<ResultData<ClassTokModel>>(resource);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result.ContinuationToken = "cancelled";
            }

            if (result == null)
            {
                result = new ResultData<ClassTokModel>();
            }
            return result;
        }
        public async Task<bool> AddClassToksToClassSetAsync(string classsetId, string pk, List<string> classtokIds, List<string> classtokPks)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", pk);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("tokids", JsonConvert.SerializeObject(classtokIds));
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("tokpks", JsonConvert.SerializeObject(classtokPks));
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classtokstoset/{classsetId}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            HttpResponseMessage response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent("", Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteClassToksFromClassSetAsync(ClassSetModel classset, List<string> classtokIds)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Remove("pk");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("pk", classset.PartitionKey);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("tokids", JsonConvert.SerializeObject(classtokIds));
            var apiUrl = $"{Config.Configurations.ApiPrefix}/classtoksfromset/{classset.Id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            HttpResponseMessage response = await _httpClientHelper.Instance.DeleteAsync(apiUrl);
            var content = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        #endregion
    }
}
