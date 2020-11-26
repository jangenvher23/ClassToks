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
using V4Fragment = Android.Support.V4.App.Fragment;
using V4FragmentManager = Android.Support.V4.App.FragmentManager;

namespace Tokkepedia.Adapters
{
    public class AdapterStatePagerFragment : Android.Support.V4.App.FragmentStatePagerAdapter
    {
        List<V4Fragment> fragments;
        List<string> fragmentTitles;

        public AdapterStatePagerFragment(V4FragmentManager fm, List<V4Fragment> _fragments, List<string> _fragmentTitles) : base(fm)
        {
            fragments = _fragments;
            fragmentTitles = _fragmentTitles;
        }

        public void AddFragment(V4Fragment fragment, System.String title)
        {
            fragments.Add(fragment);
            fragmentTitles.Add(title);
        }


        public void InsertFragmentAtPosition(V4Fragment fragment, int position)
        {
            fragments.Insert(position, fragment);
        }

        public void RemoveFragment(V4Fragment fragment)
        {
            fragments.Remove(fragment);
        }

        public override V4Fragment GetItem(int position)
        {
            return fragments[position];
        }

        public override int Count
        {
            get { return fragments.Count; }
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            return new Java.Lang.String(fragmentTitles[position]);
        }

        public override int GetItemPosition(Java.Lang.Object item)
        {
            int idx = fragments.IndexOf((V4Fragment)item);
            return idx < 0 ? PositionNone : idx;
        }


    }
}