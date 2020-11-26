using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Models;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Shared.Services.Interfaces
{
    public interface ITokMojiService
    {
        Task<ResultData<Tokmoji>> GetTokmojisAsync(string paginationId = null);
        Task<ResultModel> PurchaseTokmojiAsync(string tokmojiid, string itemLabel);
    }
}
