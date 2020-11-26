using Newtonsoft.Json;
using System;
using Tokkepedia.Shared.Models;
using Xamarin.Forms;

namespace ClassToksV2.Models
{
    public class ClassSetXF : ClassSetModel
    {
        [JsonIgnore]
        public Color ColorXF { get; set; }

        [JsonIgnore]
        public bool IsAddMode { get; set; }
    }
}