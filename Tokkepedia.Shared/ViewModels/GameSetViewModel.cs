using System.Collections.Generic;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokket.Tokkepedia;

namespace Tokkepedia.Shared.ViewModels
{
    public class GameSetViewModel
    {
        public GameSetModel GameSet { get; set; }
        public List<TokTypeList> TokGroups { get; set; }
        public string TokGroupDataString { get; set; }
        public string Base64Image { get; set; }

        public TokTypeList TokGroup { get; set; }
        public GameScheme GameScheme { get; set; } = GameScheme.TokBlitz;
    }
}
