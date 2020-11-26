using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Models;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Shared.Services.Interfaces
{
    public interface IAccountService
    {
        Task<ResultModel> Login(LoginModel model);
        Task<ResultModel> VerifyTokenAsync(string token, string refreshtoken);
        ResultModel VerifyToken(string token, string refreshtoken);
        Task<TokketUser> GetUserAsync(string id);
        TokketUser GetUser(string id);
        Task<ResultModel> SignUpAsync(string email, string password, string displayName, string country, DateTime date, string userPhoto, string accountType, string groupType, string ownerName);
        Task<bool> UpdateUserBioAsync(string bio);
        Task<bool> UpdateUserWebsiteAsync(string website);
        Task<string> UploadProfilePictureAsync(string base64);
        Task<string> UploadProfileCoverAsync(string base64);
        Task<bool> UpdateUserDisplayNameAsync(string displayName);
        Task<ResultModel> SendPasswordResetAsync(string email);
        Task<ResultData<TokketSubaccount>> GetSubaccountsAsync(string userId, string continuation = null);
        Task<TokketSubaccount> GetSubaccountAsync(string id, string userId);
        Task<ResultData<TokketTitle>> GetGenericTitlesAsync(string paginationId);
        Task<ResultData<TokketTitle>> GetTitlesAsync(string userId, string kind);
        Task<bool> SelectTitleAsync(string id);
        Task<bool> LoginSubaccountAsync(string userId, string subaccountId, string subaccountKey);
        Task<ResultModel> ChangePasswordAsync(string userId, string oldPassword, string newPassword);


        Task<bool> DeleteUserAsync(string id);

    }
}
