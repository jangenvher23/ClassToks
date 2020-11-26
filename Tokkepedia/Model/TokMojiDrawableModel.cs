using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Tokket.Tokkepedia;

namespace Tokkepedia.Model
{
    public class TokMojiDrawableModel : Tokmoji
    {
        public string TokmojiId { get; set; }
        //public Drawable TokmojImg { get; set; }
        public string TokmojImgBase64 { get; set; }
        public string PurchaseIds { get; set; }
    }
}