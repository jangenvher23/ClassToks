using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;

namespace Tokkepedia
{
    [Register("Tokkepedia.LockableViewPager")]
    public class LockableViewPager : ViewPager
    {
        public bool SwipeLocked { get; set; }

        Context mContext;

        public LockableViewPager(Context context) : base(context)
        {
            Init(context, null);
        }
        public LockableViewPager(Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
            SwipeLocked = true;
            Init(context, attrs);
        }
        private void Init(Context ctx, Android.Util.IAttributeSet attrs)
        {
            mContext = ctx;
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            if (this.SwipeLocked)
            {
                return base.OnTouchEvent(ev);
            }
            return false;
        }

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            if (this.SwipeLocked)
            {
                return base.OnInterceptTouchEvent(e);
            }
            return false;
        }

        // For ViewPager inside another ViewPager
        public override bool CanScrollHorizontally(int direction)
        {
            return this.SwipeLocked && base.CanScrollHorizontally(direction);
        }
    }
}