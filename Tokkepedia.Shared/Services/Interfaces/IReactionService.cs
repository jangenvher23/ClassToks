using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Shared.Services.Interfaces
{
    public interface IReactionService
    {
        Task<ResultModel> AddReaction(ReactionModel item);
        Task<bool> UpdateReaction(TokkepediaReaction item);
        Task<bool> DeleteReaction(string id);
        Task<ResultData<ReactionModel>> GetReactionsAsync(ReactionQueryValues values = null);
        Task<ReactionValueModel> GetReactionsValueAsync(string id);
        Task<List<TokketUserReaction>> GetReactionsUsersAsync(ReactionQueryValues reactionQueryValues);
        Task<ResultData<ReactionModel>> GetCommentReplyAsync(ReactionQueryValues values = null);
        Task<TokkepediaReaction> GetCommentAsync(string id);
    }
}
