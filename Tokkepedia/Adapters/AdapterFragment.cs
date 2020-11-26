using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using V4Fragment = Android.Support.V4.App.Fragment;
using V4FragmentManager = Android.Support.V4.App.FragmentManager;
using Android.Support.V4.App;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Fragments;

namespace Tokkepedia.Adapters
{
    public class AdapterFragment : Android.Support.V4.App.FragmentPagerAdapter
    {
        //List<V4Fragment> fragments = new List<V4Fragment>();
        //List<string> fragmentTitles = new List<string>();
        List<V4Fragment> fragments;
        List<string> fragmentTitles;

        public AdapterFragment(V4FragmentManager fm, List<V4Fragment> _fragments, List<string> _fragmentTitles) : base(fm)
        {
            fragments = _fragments;
            fragmentTitles = _fragmentTitles;
        }

        public void AddFragment(V4Fragment fragment, String title)
        {
            fragments.Add(fragment);
            fragmentTitles.Add(title);
        }

        public override V4Fragment GetItem(int position)
        {
            //if (Settings.ActivityInt == Convert.ToInt16(ActivityType.ReactionValuesActivity))
            //{
            //    switch (position)
            //    {
            //        case 0: // Fragment # 0 - This will show FirstFragment
            //            return reactionvalues_users_fragment.newInstance("All");
            //        case 1: // Fragment # 0 - This will show FirstFragment different title
            //            return reactionvalues_users_fragment.newInstance("GemA");
            //        case 2: // Fragment # 1 - This will show SecondFragment
            //            return reactionvalues_users_fragment.newInstance("GemB");
            //        case 3:
            //            return reactionvalues_users_fragment.newInstance("GemC");
            //        case 4:
            //            return reactionvalues_users_fragment.newInstance("Accurate");
            //        case 5:
            //            return reactionvalues_users_fragment.newInstance("Inaccurate");
            //        default:
            //            return null;
            //    }
            //}
            //else
            //{
                return fragments[position];
            //}
        }

        public override int Count
        {
            get { return fragments.Count; }
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            return new Java.Lang.String(fragmentTitles[position]);
        }
    }
}