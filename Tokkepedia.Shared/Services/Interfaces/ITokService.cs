using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tokkepedia.Shared.Models;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Shared.Services.Interfaces
{
    public interface ITokService
    {
        Task<List<TokModel>> GetAllToks(TokQueryValues values);
        Task<ResultData<TokSection>> GetTokSectionsAsync(string tokId, int count = 0, string continuationToken = null);
        Task<bool> CreateTokSectionAsync(TokSection tokSection, string tokId, int partitionNumber);
        Task<bool> UpdateTokSectionAsync(TokSection newTokSection);
        Task<bool> DeleteTokSectionAsync(TokSection tokSection);
        Task<List<TokModel>> GetAllFeaturedToks();
        Task<List<TokModel>> GetAllFeaturedToksAsync();
        Task<ResultModel> CreateTokAsync(TokModel tok, CancellationToken cancellationToken = default(CancellationToken));
        Task<ResultModel> DeleteTokAsync(string id);
        Task<ResultModel> UpdateTokAsync(TokModel tok, CancellationToken cancellationToken = default(CancellationToken));
        Task<TokModel> GetTokIdAsync(string id);
        Task<List<TokModel>> GetToksAsync(TokQueryValues values = null);
        Task<List<TokModel>> GetToksByIdsAsync(List<string> ids);
        Task<ResultData<TokkepediaReaction>> UserReactionsGet(string item_id);
        List<TokModel> AlternateToks(List<TokModel> resultData);
    }
}
