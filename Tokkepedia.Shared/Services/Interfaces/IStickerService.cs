using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Models;
using Tokket.Tokkepedia;

namespace Tokkepedia.Shared.Services.Interfaces
{
    public interface IStickerService
    {
        Task<List<Sticker>> GetStickersAsync();
        Task<List<PurchasedSticker>> GetStickersUserAsync(string userId);
        Task<bool> AddStickerToTokAsync(string tokId, string newStickerId);
        Task<bool> RemoveStickerFromAsync(string tokId, string stickerId);
        Task<ResultModel> PurchaseStickerAsync(string stickerId, string tokId);
    }
}
