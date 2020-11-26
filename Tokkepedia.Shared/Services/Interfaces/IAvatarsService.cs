using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Shared.Services.Interfaces
{
    public interface IAvatarsService
    {
        Task<ResultData<Avatar>> GetAvatarsAsync(string paginationId = null);
        Task<bool> SelectAvatarAsync(string avatarid);
        Task<bool> UseAvatarAsProfilePictureAsync(bool isAvatarProfilePicture);
        Task<bool> UserSelectAvatarAsync(string avatarId);
        Task<ResultData<Avatar>> AvatarsByIdsAsync(List<string> ids);
    }
}
