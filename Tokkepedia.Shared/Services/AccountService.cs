using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services.Interfaces;
using Tokket.Tokkepedia;
using Tokkepedia.Shared.Helpers.Interfaces;
using System.Globalization;
using Xamarin.Essentials;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Shared.Services
{
    public class AccountService : IAccountService
    {
        public static IAccountService Instance = new AccountService();
        private HttpClientHelper _httpClientHelper;
        public AccountService()
        {
            _httpClientHelper = new HttpClientHelper(Config.Configurations.BaseUrl);
        }
        public async Task<ResultModel> Login(LoginModel model)
        {
            TokketUser user = new TokketUser() { Email = model.Username, PasswordHash = model.Password};
            ResultModel result = new ResultModel() { ResultEnum = Result.Failed };

            var apiUrl = $"{Config.Configurations.ApiPrefix}/login"; // Api Method to Call with values
            apiUrl += $"{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}"; // Add Suffix for API

            var content = await _httpClientHelper.PostAsync(apiUrl, user);
            result = JsonConvert.DeserializeObject<ResultModel>(content);
            return result;
        }
        // Verifies user token
        public async Task<ResultModel> VerifyTokenAsync(string token, string refreshToken)
        {
            FirebaseTokenModel model = new FirebaseTokenModel();
            model.IdToken = token;
            model.RefreshToken = refreshToken;
            model.HashKey = HashGenerator.GenerateCustomHash("T0KK3T" + Settings.GetUserModel().UserId, 512, 24);
            model.UserId = Settings.GetUserModel().UserId;

            var apiUrl = $"{Config.Configurations.ApiPrefix}/verifytoken{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            //var content = await _httpClientHelper.PostAsync(apiUrl, model);
            var content = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));

            var item = await content.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ResultModel>(item);
        }
        public ResultModel VerifyToken(string token, string refreshToken)
        {
            var userid = Settings.GetUserModel().UserId;
            FirebaseTokenModel model = new FirebaseTokenModel();
            model.IdToken = token;
            model.RefreshToken = refreshToken;
            model.HashKey = HashGenerator.GenerateCustomHash("T0KK3T" + userid, 512, 24);
            model.UserId = Settings.GetUserModel().UserId;

            var apiUrl = $"{Config.Configurations.ApiPrefix}/verifytoken{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            var content = _httpClientHelper.Post(apiUrl, model);
            return JsonConvert.DeserializeObject<ResultModel>(content);
        }
        public async Task<TokketUser> GetUserAsync(string id)
        {
            var apiUrl = $"{Config.Configurations.ApiPrefix}/user/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}"; // Api Method to Call with values
            var content = await _httpClientHelper.GetAsync(apiUrl);
            return JsonConvert.DeserializeObject<TokketUser>(content);
        }
        public TokketUser GetUser(string id)
        {
            var apiUrl = $"{Config.Configurations.ApiPrefix}/user/{id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}"; // Api Method to Call with values
            var content = _httpClientHelper.GetData(apiUrl);
            return JsonConvert.DeserializeObject<TokketUser>(content);
        }
        //public async Task<ResultModel> SignUpAsync(string email, string password, string displayName, string country, DateTime date, string userPhoto)
        //{
        //    TokketUser user = new TokketUser() { Email = email, PasswordHash = password, DisplayName = displayName, Country = country, Birthday = date, UserPhoto = userPhoto };
        //    user.BirthDate = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month)} {date.Day}";
        //    var apiUrl = $"{Config.Configurations.ApiPrefix}/signup"; // Api Method to Call with values
        //    apiUrl += $"{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}"; // Add Suffix for API
        //    var content = await _httpClientHelper.PostAsync(apiUrl, user);
        //    return JsonConvert.DeserializeObject<ResultModel>(content);
        //}
        public async Task<ResultModel> SignUpAsync(string email, string password, string displayName, string country, DateTime date, string userPhoto, string accountType, string groupType, string ownerName)
        {
            TokketUser user = new TokketUser();
            user.Email = email;
            user.PasswordHash = password;
            user.DisplayName = displayName;
            user.Country = country;
            user.Birthday = date;
            if (accountType == "individual")
            {
                user.UserPhoto = userPhoto;
            }
            else
            {
                user.SubaccountPhoto = userPhoto;
                user.AccountType = accountType;
                user.GroupAccountType = groupType;
                user.SubaccountName = ownerName;
            }
            user.BirthDate = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month)} {date.Day}";
            _httpClientHelper.ClearHeaders();
            var apiUrl = $"{Config.Configurations.ApiPrefix}/signup"; // Api Method to Call with values
            apiUrl += $"{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}"; // Add Suffix for API
            var res = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));
            var content = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ResultModel>(content);
        }
        public async Task<bool> UpdateFollowAsync(TokkepediaFollow tok)
        {
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);

            var apiUrl = $"{Config.Configurations.ApiPrefix}/follow/{tok.Id}{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.PutAsync(apiUrl, tok);
            //return response.IsSuccessStatusCode;
            return true;
        }
        public async Task<bool> UpdateUserBioAsync(string bio)
        {
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);

            var apiUrl = $"{Config.Configurations.ApiPrefix}/userbio{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.PostAsync(apiUrl, bio);
            //return response.IsSuccessStatusCode;
            return true;
        }
        public async Task<bool> UpdateUserWebsiteAsync(string website)
        {
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);

            var apiUrl = $"{Config.Configurations.ApiPrefix}/userwebsite{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.PostAsync(apiUrl, website);
            //return response.IsSuccessStatusCode;
            return true;
        }
        #region Image
        public async Task<string> UploadProfilePictureAsync(string base64)
        {
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/profilepicture{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.PostAsync(apiUrl, base64);
            return response;
            //return await response.Content.ReadAsAsync<string>();
        }
        public async Task<string> UploadProfileCoverAsync(string base64)
        {
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);

            var apiUrl = $"{Config.Configurations.ApiPrefix}/profilecover{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.PostAsync(apiUrl, base64);
            return response;
            //return await response.Content.ReadAsAsync<string>();
        }
        public async Task<bool> UpdateUserDisplayNameAsync(string displayName)
        {
            var userid = Settings.GetUserModel().UserId;
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userid);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);

            var apiUrl = $"{Config.Configurations.ApiPrefix}/userdisplayname{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            var response = await _httpClientHelper.PostAsync(apiUrl, displayName);
            return true;
            //return response.IsSuccessStatusCode;
        }
        #endregion
        public async Task<ResultModel> SendPasswordResetAsync(string email)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("email", email);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/sendpasswordreset"; // Api Method to Call with values
            apiUrl += $"{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}"; // Add Suffix for API
            var response = await _httpClientHelper.PostAsync(apiUrl, email);

            ResultModel resultModel = new ResultModel();
            if (response.Contains("error"))
            {
                resultModel.ResultEnum = Result.Failed;
                resultModel.ResultMessage = "Email not found.";
            }
            else
            {
                resultModel.ResultEnum = Result.Success;
            }

            return resultModel;
            //return response.IsSuccessStatusCode;
        }
        public async Task<ResultData<TokketSubaccount>> GetSubaccountsAsync(string userId, string continuation = null)
        {
            _httpClientHelper.ClearHeaders();
            var apiUrl = $"{Config.Configurations.ApiPrefix}/subaccounts/{userId}?serviceid={Config.Configurations.ServiceId}"; // Api Method to Call with values
            apiUrl += $"&code={Config.Configurations.ApiKey}"; // Add Suffix for API
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);
            var item = await response.Content.ReadAsStringAsync();

            var dataresource = JsonConvert.DeserializeObject<ResultModel>(item);
            return JsonConvert.DeserializeObject<ResultData<TokketSubaccount>>(dataresource.ResultObject.ToString());
        }
        public async Task<TokketSubaccount> GetSubaccountAsync(string id, string userId)
        {
            _httpClientHelper.ClearHeaders();
            var apiUrl = $"{Config.Configurations.ApiPrefix}/subaccount/{id}?userid={userId}&serviceid={Config.Configurations.ServiceId}"; // Api Method to Call with values
            apiUrl += $"&code={Config.Configurations.ApiKey}"; // Add Suffix for API
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);
            var item = await response.Content.ReadAsStringAsync();
            var dataresource = JsonConvert.DeserializeObject<ResultModel>(item);
            return JsonConvert.DeserializeObject<TokketSubaccount>(dataresource.ResultObject.ToString());
        }
        public async Task<ResultData<TokketTitle>> GetGenericTitlesAsync(string paginationId)
        {
            var apiUrl = $"{Config.Configurations.ApiPrefix}/titlesgeneric{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            _httpClientHelper.ClearHeaders();
            if (!string.IsNullOrEmpty(paginationId))
                _httpClientHelper.Instance.DefaultRequestHeaders.Add("pagination_id", paginationId);

            HttpResponseMessage response = await _httpClientHelper.Instance.GetAsync(apiUrl).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultData<TokketTitle>>(content);
            return result;
        }
        public async Task<ResultData<TokketTitle>> GetTitlesAsync(string userId, string kind)
        {
            _httpClientHelper.ClearHeaders();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("kind", kind);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/titles/{userId}?serviceid={Config.Configurations.ServiceId}"; // Api Method to Call with values
            apiUrl += $"&code={Config.Configurations.ApiKey}"; // Add Suffix for API
            //_client.BaseAddress = new Uri($"{baseUrl}/subaccount/{id}?userid={userId}&serviceid={serviceId}&code={_apiKey}");
            var response = await _httpClientHelper.Instance.GetAsync(apiUrl);
            var content = await response.Content.ReadAsStringAsync();
            var item =  JsonConvert.DeserializeObject<ResultModel>(content);
            return JsonConvert.DeserializeObject<ResultData<TokketTitle>>(item.ResultObject.ToString());
        }

        public async Task<bool> SelectTitleAsync(string id)
        {
            _httpClientHelper.ClearHeaders();
            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", Settings.GetUserModel().UserId);

            var apiUrl = $"{Config.Configurations.ApiPrefix}/titleselect/{id}"; // Api Method to Call with values
            apiUrl += $"{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";

            HttpResponseMessage response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginSubaccountAsync(string userId, string subaccountId, string subaccountKey)
        {
            _httpClientHelper.Instance.DefaultRequestHeaders.Clear();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("serviceid", "tokkepedia"); //Valid: "tokket", "tokblitz", "tokkepedia"
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("deviceplatform", "android"); //Valid: "web", "android", "ios"

            var idtoken = await SecureStorage.GetAsync("idtoken");
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("token", idtoken);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", userId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("subaccountid", subaccountId);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("subaccountkey", subaccountKey);
            
            var apiUrl = $"{Config.Configurations.ApiPrefix}/subaccountlogin/"; // Api Method to Call with values
            apiUrl += $"{Config.Configurations.CodePrefix}{Config.Configurations.ApiKey}";
            HttpResponseMessage response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(JsonConvert.SerializeObject(Settings.GetTokketUser()), Encoding.UTF8, "application/json"));

            var item = await response.Content.ReadAsStringAsync();
            var dataresource = JsonConvert.DeserializeObject<ResultModel>(item);
            return dataresource.ResultEnum == Result.Success ? true : false;
        }

        public async Task<ResultModel> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            _httpClientHelper.Instance.DefaultRequestHeaders.Clear();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("oldPassword", oldPassword);
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("newPassword", newPassword);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/changepassword/{userId}"; // Api Method to Call with values
            HttpResponseMessage response = await _httpClientHelper.Instance.PostAsync(apiUrl, new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ResultModel>(content);
        }


        public async Task<bool> DeleteUserAsync(string id)
        {
            _httpClientHelper.Instance.DefaultRequestHeaders.Clear();
            _httpClientHelper.Instance.DefaultRequestHeaders.Add("userid", id);
            var apiUrl = $"{Config.Configurations.ApiPrefix}/user/{id}"; // Api Method to Call with values     
            HttpResponseMessage response = await _httpClientHelper.Instance.DeleteAsync(apiUrl);
            return response.IsSuccessStatusCode;
        }

    }
}
