using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.Graphics;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide.Load;
using Com.Bumptech.Glide.Load.Engine;
using Com.Bumptech.Glide.Request;
using Com.Bumptech.Glide.Request.Target;
using Java.Lang;

namespace Tokkepedia.Helpers
{
    public class GlideImgListener : Java.Lang.Object, IRequestListener
    {
        public int mColorPalette { get; set; }
        public Activity ParentActivity { get; set; }
        public bool OnLoadFailed(GlideException p0, Java.Lang.Object p1, ITarget p2, bool p3)
        {
            ParentActivity.StartPostponedEnterTransition();
            return false;
        }

        public bool OnResourceReady(Java.Lang.Object resource, Java.Lang.Object model, ITarget target, DataSource dataSource, bool isFirstResource)
        {
            ParentActivity.StartPostponedEnterTransition();
            if (resource != null)
            {
                Bitmap bmResource = (resource as BitmapDrawable).Bitmap;
                Palette p = Palette.From(bmResource).Generate();
                // Use generated instance
                mColorPalette = p.GetMutedColor(ContextCompat.GetColor(ParentActivity,Resource.Color.primary_dark));
            }

            return false;
        }
    }
}