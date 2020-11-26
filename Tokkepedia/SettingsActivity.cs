using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Tokkepedia.Adapters;                   

namespace Tokkepedia
{
#if (_TOKKEPEDIA)
    [Activity(Label = "Settings", Theme = "@style/CustomAppTheme")]
#endif

#if (_CLASSTOKS)
    [Activity(Label = "Settings", Theme = "@style/CustomAppThemeBlue")]
#endif
    public class SettingsActivity : BaseActivity
    {
#if (_TOKKEPEDIA)
        string[] items = { "Personal Data", "Change Password", "Privacy Policy","Terms of Service","About Tokkepedia","Contact", "More Settings", "Log Out" };
#endif

#if (_CLASSTOKS)
        string[] items = { "Personal Data", "Change Password", "Privacy Policy", "Terms of Service", "About Class Toks", "Contact", "More Settings", "Log Out" };
#endif
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.settings_page);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            var adapter = new SettingsAdapter(this, items);
            var listview = FindViewById<ListView>(Resource.Id.listViewSettings);
            listview.Adapter = adapter;
            listview.ItemClick += adapter.OnListItemClick;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}