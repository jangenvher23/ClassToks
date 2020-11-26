using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Models;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Shared.Services.Interfaces
{
    public interface ICommonService
    {
        Task<ResultData<TokketUser>> SearchUsersAsync(string text, string token = "");
        Task<ResultData<Category>> SearchCategoriesAsync(string text);
        Task<bool> AddRecentSearchAsync(string text);
        Task<UserSearches> GetRecentSearchesAsync();
        Task<string> UploadImageAsync(string base64);
        Task<OggClass> GetQuoteAsync();
    }
}
