using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using V4Fragment = Android.Support.V4.App.Fragment;
using V4FragmentManager = Android.Support.V4.App.FragmentManager;

namespace Tokkepedia.Adapters
{
    public class AdapterStateFragment : Android.Support.V4.App.FragmentStatePagerAdapter
    {
        List<V4Fragment> fragments;
        List<string> fragmentTitles;

        public AdapterStateFragment(V4FragmentManager fm, List<V4Fragment> _fragments, List<string> _fragmentTitles) : base(fm)
        {
            fragments = _fragments;
            fragmentTitles = _fragmentTitles;
        }

        #region implemented abstract members of PagerAdapter

        public override int Count
        {
            get
            {
                return fragments.Count;
            }
        }

        #endregion

        #region implemented abstract members of FragmentStatePagerAdapter

        public override V4Fragment GetItem(int position)
        {
            return fragments[position];
        }
        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            return new Java.Lang.String(fragmentTitles[position]);
        }
        #endregion
    }
}