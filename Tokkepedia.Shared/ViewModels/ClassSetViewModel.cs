using System;
using System.Collections.Generic;
using System.Text;
using Tokkepedia.Shared.Models;

namespace Tokkepedia.Shared.ViewModels
{
    public class ClassSetViewModel
    {
        public ClassSetModel ClassSet { get; set; }
        public List<ClassTokModel> ClassToks { get; set; }
        public string Token { get; set; }
        public bool IsSignedIn { get; set; }
    }
}
