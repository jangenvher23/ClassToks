using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tokket.Tokkepedia;

namespace Tokkepedia.Shared.Models
{
    public class ClassSetModel : Set
    {
        //#region Set Format
        //[JsonIgnore]
        //private string[] validFormats = new string[] { "Basic", "Detailed", "Mega" };
        //[JsonIgnore]
        //private string tokFormat = "Basic";
        ///// <summary>In a class tok, Tok Groups are Tok Formats.</summary>
        //[JsonProperty(PropertyName = "tok_group")]
        //public new string TokGroup
        //{
        //    get { return tokFormat; }
        //    set
        //    {
        //        if (validFormats.Contains(value))
        //            tokFormat = value;
        //        else
        //            tokFormat = "Detailed";
        //    }
        //}
        //#endregion
        #region Privacy
        /// <summary>Determines if tok is private.</summary>
        [JsonProperty(PropertyName = "is_private", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPrivate { get; set; } = true;
        #endregion
        #region Group
        /// <summary>Determines if tok is private.</summary>
        [JsonProperty(PropertyName = "group_id", NullValueHandling = NullValueHandling.Ignore)]
        public string GroupId { get; set; } = null;


        [JsonProperty(PropertyName = "group", NullValueHandling = NullValueHandling.Ignore)]
        public ClassGroupModel Group { get; set; }
        #endregion
    }
}