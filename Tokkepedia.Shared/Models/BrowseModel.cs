using System;
using System.Collections.Generic;
using System.Text;
using Tokket.Tokkepedia;

namespace Tokkepedia.Shared.Models
{
    public class BrowseModel
    {
        public List<TokTypeList> TokGroups { get; set; }
        public List<string> TokTypes { get; set; }
        public List<TokTypeListCounter> Counters { get; set; }
    }
}
