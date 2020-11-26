using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tokkepedia.Shared.Models
{
    public class ClassTokModel : TokModel
    {
        #region Privacy
        /// <summary>Determines if tok is private.</summary>
        [JsonProperty(PropertyName = "is_private", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsPrivate { get; set; } = true;
        /// <summary>Determines if tok is public.</summary>
        
        public bool IsPublic { get; set; } = false;
        /// <summary>Determines if tok is group.</summary>
        public bool IsGroup { get; set; } = false;
        #endregion

        #region Group
        /// <summary>Only add if this content is part of a group. </summary>
        [JsonProperty(PropertyName = "group_id", NullValueHandling = NullValueHandling.Ignore)]
        public string GroupId { get; set; } = null;

        /// <summary>Name of the group belong </summary>
        [JsonProperty(PropertyName = "group_name", NullValueHandling = NullValueHandling.Ignore)]
        public string GroupName { get; set; } = null;
        #endregion
    }
}
