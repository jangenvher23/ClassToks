using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Shared.Services.Interfaces
{
    public interface IClassService
    {
        Task<ClassGroupModel> AddClassGroupAsync(ClassGroupModel item);
        Task<bool> UpdateClassGroupAsync(ClassGroupModel item);
        Task<bool> DeleteClassGroupAsync(string id, string pk);
        Task<ClassGroupModel> GetClassGroupAsync(string id);
        Task<ResultData<ClassGroupModel>> GetClassGroupAsync(ClassGroupQueryValues queryValues);
        Task<ClassGroupRequestModel> GetClassGroupRequestAsync(string id, string pk);
        Task<ClassGroupRequestModel> RequestClassGroupAsync(ClassGroupRequestModel item);
        Task<ResultData<ClassGroupRequestModel>> GetClassGroupJoinRequests(string continuationtoken, string groupid, RequestStatus requestStatus = RequestStatus.Pending);
        Task<ResultData<ClassGroupModel>> GetClassGroupRequestAsync(ClassGroupRequestQueryValues queryValues);
        Task<bool> AcceptRequest(string id, string pk, ClassGroupModel model);
        Task<bool> DeclineRequest(string id, string pk, ClassGroupModel model);
        Task<bool> LeaveClassGroupAsync(string id, string pk);
        Task<ResultData<TokketUser>> GetGroupMembers(string groupid, string paginationid = "");
        Task<bool> AddClassSetAsync(ClassSetModel item, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> UpdateClassSetAsync(ClassSetModel item, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> DeleteClassSetAsync(string id, string pk);
        Task<ClassSetModel> GetClassSetAsync(string id, string pk);
        Task<ResultData<ClassSetModel>> GetClassSetAsync(ClassSetQueryValues queryValues, CancellationToken cancellationToken = default(CancellationToken));
        Task<ResultModel> AddClassToksAsync(ClassTokModel item, CancellationToken cancellationToken = default(CancellationToken));
        Task<ResultModel> UpdateClassToksAsync(ClassTokModel item, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> DeleteClassToksAsync(string id, string pk);
        Task<ClassTokModel> GetClassToksAsync(string id, string pk);
        Task<ResultData<ClassTokModel>> GetClassToksAsync(ClassTokQueryValues queryValues, CancellationToken token = default(CancellationToken));
        Task<List<CommonModel>> GetMoreFilterOptions(ClassTokQueryValues queryValues);
        Task<bool> AddClassToksToClassSetAsync(string classsetId, string pk, List<string> classtokIds, List<string> classtokPks);
        Task<bool> DeleteClassToksFromClassSetAsync(ClassSetModel classset, List<string> classtokIds);
    }
}
