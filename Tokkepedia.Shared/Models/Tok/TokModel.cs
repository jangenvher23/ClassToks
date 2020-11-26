using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models.Base;
using Tokket.Tokkepedia;

namespace Tokkepedia.Shared.Models
{
    public class TokViewModel : Tok
    {

        [JsonIgnore]
        public string ColorHex { get; set; }

        public int IndexCounter { get; set; }

        #region SubAccount and Title
        //
        // Summary:
        //     True if the currently selected subaccount is the owner.
        [JsonProperty(PropertyName = "subaccount_owner", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SubaccountOwner { get; set; }
        //
        // Summary:
        //     Currently selected subaccount's profile picture.
        [JsonProperty(PropertyName = "subaccount_photo", NullValueHandling = NullValueHandling.Ignore)]
        public string SubaccountPhoto { get; set; }
        //
        // Summary:
        //     Currently selected subaccount's display name.
        [JsonProperty(PropertyName = "subaccount_name", NullValueHandling = NullValueHandling.Ignore)]
        public string SubaccountName { get; set; }
        //
        // Summary:
        //     Currently selected subaccount's id.
        [JsonProperty(PropertyName = "subaccount_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SubaccountId { get; set; }
        //
        // Summary:
        //     User's account type (group_account_type). Can only be "family" or "organization"
        [JsonProperty(PropertyName = "group_account_type", NullValueHandling = NullValueHandling.Ignore)]
        public string GroupAccountType { get; set; }
        //
        // Summary:
        //     True if the user's currently selected title is enabled.
        [JsonProperty(PropertyName = "title_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? TitleEnabled { get; set; }
        //
        // Summary:
        //     True if the user's currently selected title is unique.
        [JsonProperty(PropertyName = "title_unique", NullValueHandling = NullValueHandling.Ignore)]
        public bool? TitleUnique { get; set; }
        //
        // Summary:
        //     Display of the user's currently selected title. Case sensitive and max of 25
        //     characters
        [JsonProperty(PropertyName = "title_display", NullValueHandling = NullValueHandling.Ignore)]
        public string TitleDisplay { get; set; }
        //
        // Summary:
        //     Id of the user's currently selected title. Not case sensitive and max of 25 characters
        [JsonProperty(PropertyName = "title_id", NullValueHandling = NullValueHandling.Ignore)]
        public string TitleId { get; set; }
        //
        // Summary:
        //     User's account type (account_type). Can only be "individual" or "group"
        [JsonProperty(PropertyName = "account_type")]
        public string AccountType { get; set; }
        #endregion
    }


}
