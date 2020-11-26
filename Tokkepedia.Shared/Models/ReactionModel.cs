using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Tokkepedia.Shared.Helpers;
using Tokket.Tokkepedia;

namespace Tokkepedia.Shared.Models
{
    public class ReactionModel : TokkepediaReaction
    {
        [JsonProperty(PropertyName = "children_token")]
        public string ChildrenToken { get; set; }
    }
}
