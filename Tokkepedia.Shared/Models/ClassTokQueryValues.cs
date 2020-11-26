using System;
using System.Collections.Generic;
using System.Text;
using Tokkepedia.Shared.Helpers;

namespace Tokkepedia.Shared.Models
{
    public class ClassTokQueryValues
    {
        public int limit = 20;
        public string kind = "";
        public string groupid = "";
        public string userid = "";
        public string paginationid = null;
        public string partitionkeybase = "";
        public string toktypeid = "";
        public string classsetid = "";
        public long? itemtotal = 0;

        public string category { get; set; }
        public string tokgroup { get; set; }
        public string toktype { get; set; }

        public bool? startswith { get; set; }
        public string text { get; set; }

        public bool? publicfeed { get; set; }

        //Search
        public string searchkey { get; set; } = null;
        public string searchvalue { get; set; } = null;

        #region Filter By
        public FilterBy FilterBy = FilterBy.None;
        public bool RecentOnly = true; // Alphabetical Order if false.
        public List<string> FilterItems = new List<string>();
        #endregion
    }
}
