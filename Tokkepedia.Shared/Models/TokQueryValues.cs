using System;
using System.Collections.Generic;
using System.Text;

namespace Tokkepedia.Shared.Models
{
    /// <summary>
    /// Values for querying toks.
    /// </summary>
    public class TokQueryValues
    {
        public string order { get; set; }
        public string country { get; set; }
        public string category { get; set; }
        public string tokgroup { get; set; }
        public string toktype { get; set; }
        public string userid { get; set; }
        public string itemid { get; set; }
        public string loadmore { get; set; }
        public string token { get; set; }
        public string streamtoken { get; set; }
        public string detailnumber { get; set; } = "-1";
        public string offset { get; set; }
        public string toktotal { get; set; }
        public bool? image { get; set; }
        public string tagid { get; set; }
        public string sortby { get; set; } = "standard";
        #region Search
        public bool? startswith { get; set; }
        public string text { get; set; }
        #endregion
    }
}
