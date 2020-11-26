using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Theartofdev.Edmodo.Cropper;
using Java.Interop;
using Uri = Android.Net.Uri;
using TokketAppCrop;
using Tokkepedia.Fragments;
using Android.Content.PM;
using Android.Support.V4.Widget;

namespace Tokkepedia
{
    [Activity(Label = "Crop Image", NoHistory = true, Theme = "@style/Theme.AppCompat")]
    public class CropImageActivity : BaseActivity
    {
        //region: Fields and Consts

        private DrawerLayout _drawerLayout;

        private ActionBarDrawerToggle _drawerToggle;

        private CropFragment _currentFragment;

        private Uri _cropImageUri;

        private CropImageViewOptions _cropImageViewOptions = new CropImageViewOptions();
        //endregion
        public void SetCurrentFragment(CropFragment fragment)
        {
            _currentFragment = fragment;
        }
        public void SetCurrentOptions(CropImageViewOptions options)
        {
            _cropImageViewOptions = options;
            UpdateDrawerTogglesByOptions(options);
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.crop_activity_main);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            _drawerLayout = (DrawerLayout)FindViewById(Resource.Id.drawer_layoutcrop);

            _drawerToggle = new ActionBarDrawerToggle(this, _drawerLayout, Resource.String.main_drawer_open, Resource.String.main_drawer_close);
            _drawerToggle.DrawerIndicatorEnabled = true;
            _drawerLayout.AddDrawerListener(_drawerToggle);

            if (savedInstanceState == null)
            {
                SetMainFragmentByPreset(CropDemoPreset.Rect);
            }
        }
        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            _drawerToggle.SyncState();
            _currentFragment.UpdateCurrentCropViewOptions();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.crop_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (_drawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }
            if (_currentFragment != null && _currentFragment.OnOptionsItemSelected(item))
            {
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == CropImage.PickImageChooserRequestCode && resultCode == Result.Ok)
            {
                var imageUri = CropImage.GetPickImageResultUri(this, data);
                if (CropImage.IsReadExternalStoragePermissionsRequired(this, imageUri))
                {
                    _cropImageUri = imageUri;

                    RequestPermissions(new[] { Manifest.Permission.ReadExternalStorage }, CropImage.PickImagePermissionsRequestCode);
                }
                else
                {

                    _currentFragment.setImageUri(imageUri);
                }
            }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == CropImage.CameraCapturePermissionsRequestCode)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    CropImage.StartPickImageActivity(this);
                }
                else
                {
                    Toast.MakeText(this, "Cancelling, required permissions are not granted", ToastLength.Long).Show();
                }
            }
            if (requestCode == CropImage.PickImagePermissionsRequestCode)
            {
                if (_cropImageUri != null && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    _currentFragment.setImageUri(_cropImageUri);
                }
                else
                {
                    Toast.MakeText(this, "Cancelling, required permissions are not granted", ToastLength.Long).Show();
                }
            }
        }

        [Java.Interop.Export("onDrawerOptionClicked")]
        public void onDrawerOptionClicked(View view)
        {
            switch (view.Id)
            {
                case Resource.Id.drawer_option_load:
                    if (CropImage.IsExplicitCameraPermissionRequired(this))
                    {
                        RequestPermissions(new[] { Manifest.Permission.Camera }, CropImage.CameraCapturePermissionsRequestCode);
                    }
                    else
                    {
                        CropImage.StartPickImageActivity(this);
                    }
                    _drawerLayout.CloseDrawers();
                    break;
                case Resource.Id.drawer_option_toggle_aspect_ratio:
                    if (!_cropImageViewOptions.FixAspectRatio)
                    {
                        _cropImageViewOptions.FixAspectRatio = true;
                        _cropImageViewOptions.AspectRatio = (1, 1);
                    }
                    else
                    {
                        if (_cropImageViewOptions.AspectRatio.AspectRatioX == 1 && _cropImageViewOptions.AspectRatio.AspectRatioY == 1)
                        {
                            _cropImageViewOptions.AspectRatio = (4, 3);
                        }
                        else if (_cropImageViewOptions.AspectRatio.AspectRatioX == 4 && _cropImageViewOptions.AspectRatio.AspectRatioY == 3)
                        {
                            _cropImageViewOptions.AspectRatio = (16, 9);
                        }
                        else if (_cropImageViewOptions.AspectRatio.AspectRatioX == 16 && _cropImageViewOptions.AspectRatio.AspectRatioY == 9)
                        {
                            _cropImageViewOptions.AspectRatio = (9, 16);
                        }
                        else
                        {
                            _cropImageViewOptions.FixAspectRatio = false;
                        }
                    }
                    _currentFragment.SetCropImageViewOptions(_cropImageViewOptions);
                    UpdateDrawerTogglesByOptions(_cropImageViewOptions);
                    break;
                case Resource.Id.drawer_option_exit:
                    Finish();
                    break;
                default:
                    Toast.MakeText(this, "Unknown drawer option clicked", ToastLength.Long).Show();
                    break;
            }
        }

        private void SetMainFragmentByPreset(CropDemoPreset demoPreset)
        {
            var fragmentManager = SupportFragmentManager;
            fragmentManager.BeginTransaction()
                    .Replace(Resource.Id.cropcontainer, CropFragment.NewInstance(demoPreset))
                    .Commit();
        }

        private void UpdateDrawerTogglesByOptions(CropImageViewOptions options)
        {
            var aspectRatio = "FREE";
            if (options.FixAspectRatio)
            {
                aspectRatio = options.AspectRatio.AspectRatioX + ":" + options.AspectRatio.AspectRatioY;
            }
            ((TextView)FindViewById(Resource.Id.drawer_option_toggle_aspect_ratio)).Text = Resources.GetString(Resource.String.drawer_option_toggle_aspect_ratio, aspectRatio);
        }
    }
}