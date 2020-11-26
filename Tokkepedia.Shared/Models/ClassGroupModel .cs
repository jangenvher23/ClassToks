using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Tokket.Tokkepedia;

namespace Tokkepedia.Shared.Models
{
    public class ClassGroupModel : BaseModel
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; } = "classgroup";

        /// <summary>Uniquely identifies the user who posted.</summary>
        [JsonRequired]
        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; } = "user";
        /// <summary>User's display name.</summary>
        [JsonProperty(PropertyName = "user_display_name", NullValueHandling = NullValueHandling.Ignore)]
        public string UserDisplayName { get; set; } = null;

        [JsonProperty(PropertyName = "user_country", NullValueHandling = NullValueHandling.Ignore)]
        public string UserCountry { get; set; } = null;

        /// <summary>User's profile image.</summary>
        [JsonProperty(PropertyName = "user_photo", NullValueHandling = NullValueHandling.Ignore)]
        public string UserPhoto { get; set; } = null;
        /// <summary>User's header image.</summary>
        [JsonProperty(PropertyName = "cover_photo", NullValueHandling = NullValueHandling.Ignore)]
        public string CoverPhoto { get; set; } = null;

        #region Color
        /// <summary>Main color in hex format (if null, then automatically or randomly selected). For toks this is the tok tile color.</summary>
        [JsonProperty(PropertyName = "color_main_hex", NullValueHandling = NullValueHandling.Ignore)]
        public string ColorMainHex { get; set; } = null;
        #endregion

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "image", NullValueHandling = NullValueHandling.Ignore)]
        public string Image { get; set; } = null;
        [JsonProperty(PropertyName = "school", NullValueHandling = NullValueHandling.Ignore)]
        public string School { get; set; } = null;
        [JsonProperty(PropertyName = "members")]
        public int Members { get; set; }
        // Used when adding to list of users joined. This allows any item in -classgroupsjoined to change/access the original item (located in {userid}-classgroups
        [JsonProperty(PropertyName = "ownerpk", NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerPartitionKey { get; set; } = null;
        // Used when adding to list of users joined. This allows any item in -classgroupsjoined to change/access -classgroupmembers
        [JsonProperty(PropertyName = "memberpk", NullValueHandling = NullValueHandling.Ignore)]
        public string MemberPartitionKey { get; set; } = null;
        [JsonProperty(PropertyName = "ismember")]
        public bool IsMember { get; set; }
        [JsonProperty(PropertyName = "haspendingrequest")]
        public bool HasPendingRequest { get; set; }
        //Example: pk: "userid-classgroups0", ownerpk = "userid-classgroups0", memberpk: "userid-classgroupmembers0"
    }
}