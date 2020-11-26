using System;
using System.Collections.Generic;
using System.Text;
using Tokkepedia.Shared.Helpers;

namespace Tokkepedia.Shared.Models
{
    public class ResultModel
    {
        public ResultModel()
        {
            ResultEnum = Result.None;
        }
        public Result ResultEnum { get; set; }
        public string ResultMessage { get; set; }
        public object ResultObject { get; set; }
    }
}
