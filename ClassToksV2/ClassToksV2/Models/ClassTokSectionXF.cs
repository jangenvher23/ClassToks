using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Tokket.Tokkepedia;

namespace ClassToksV2
{
    public class TokSectionXF : TokSection
    {
        [JsonIgnore]
        public bool IsAddMode { get; set; }


    }
}