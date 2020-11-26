﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tokkepedia.Shared.Models
{
    public class ClassGroupQueryValues
    {
        public int limit = 20;
        public string kind = "";
        public string itemid = "";
        public string userid = "";
        public string paginationid = null;
        public string partitionkeybase = "";
        public long? itemtotal = 0;
        public bool joined = false;
        public bool? showImage = null; // Both = null, Image = true, Non-Image = false
        public bool? isDescending = null;
        #region Search
        public bool? startswith { get; set; }
        public string text { get; set; }
        #endregion
    }
}